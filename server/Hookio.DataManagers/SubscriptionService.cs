using Hookio.Contracts;
using Hookio.Database.Entities;
using Hookio.Database.Interfaces;
using Hookio.Discord;
using Hookio.Discord.Contracts;
using Hookio.Enums;
using Hookio.Exceptions;
using Hookio.Twitch.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using System.Text.RegularExpressions;

namespace Hookio.Database;

public partial class SubscriptionService(
    IDbContextFactory<HookioContext> contextFactory,
    IHttpClientFactory httpClientFactory,
    ITwitchEventSubService twitchEventSub) : ISubscriptionService
{
    public async Task<SubscriptionResponse?> GetSubscription(ulong guildId, int id)
    {
        var ctx = await contextFactory.CreateDbContextAsync();

        var subscription = await ctx.Subscriptions
            .Where(x => x.GuildId == guildId && x.Id == id)
            .Include(s => s.Feed)
            .Include(s => s.Events)
                .ThenInclude(e => e.Message)
                    .ThenInclude(m => m!.Embeds.OrderBy(e => e.Index))
                        .ThenInclude(e => e.Fields.OrderBy(f => f.Index))
            .FirstOrDefaultAsync();

        return subscription is null ? null : SubscriptionMapper.ToContract(subscription);
    }

    public async Task<SubscriptionResponse?> CreateSubscription(ulong guildId, SubscriptionRequest request)
    {
        var rssUrl = request.Url;
        Feed? feed = null;
        if (request.SubscriptionType == SubscriptionType.Youtube)
        {
            var channelId = GetYoutubeChannelId(request.Url) ?? throw new InvalidChannelURLException("Invalid youtube channel ID");
            rssUrl = $"https://www.youtube.com/feeds/videos.xml?channel_id={channelId}";
        }

        using var context = await contextFactory.CreateDbContextAsync();
        var subscriptionsCount = await context.Subscriptions.Where(x => x.GuildId == guildId).CountAsync();
        if (subscriptionsCount >= 2)
        {
            throw new RequiresPremiumException("This feature requires premium");
        }

        await using var transaction = context.Database.IsRelational()
            ? await context.Database.BeginTransactionAsync()
            : null;

        if (request.SubscriptionType != SubscriptionType.Twitch)
            feed = await FeedRepository.GetOrCreateFeedAsync(context, rssUrl!);

        int length = 0;
        TwitchEventSubRegistration? twitch = null;
        try
        {
            var (webhookChannelId, webhookUrl) = await ValidateWebhook(request.WebhookUrl, guildId);

            var subscription = new Subscription
            {
                GuildId = guildId,
                WebhookUrl = webhookUrl,
                SubscriptionType = request.SubscriptionType,
                Feed = feed,
                WebhookChannel = webhookChannelId,
                Events = []
            };

            foreach (var eventRequest in request.Events)
            {
                var message = BuildMessage(eventRequest.Value.Message);
                length += eventRequest.Value.Message.Content?.Length ?? 0;
                length += eventRequest.Value.Message.Embeds.Sum(e => e.Length);

                subscription.Events.Add(new Event
                {
                    Type = eventRequest.Value.EventType,
                    Subscription = subscription,
                    Message = message
                });
            }

            if (length > 6000)
            {
                throw new EmbedTooLongException("The embeds and content length cannot be longer than 6k characters");
            }

            if (request.SubscriptionType == SubscriptionType.Twitch)
            {
                twitch = await twitchEventSub.RegisterAsync(request.Url);
                subscription.TwitchLogin = twitch.Login;
                subscription.TwitchBroadcasterId = twitch.BroadcasterId;
                subscription.TwitchEventSubIds = string.Join(',', twitch.SubscriptionIds);
            }

            context.Subscriptions.Add(subscription);
            await context.SaveChangesAsync();
            if (transaction is not null) await transaction.CommitAsync();

            var newSubscription = await context.Subscriptions
                .Where(x => x.Id == subscription.Id)
                .Include(x => x.Feed)
                .Include(x => x.Events)
                    .ThenInclude(e => e.Message)
                        .ThenInclude(m => m.Embeds)
                            .ThenInclude(e => e.Fields)
                .SingleAsync();

            return SubscriptionMapper.ToContract(newSubscription);
        }
        catch
        {
            if (twitch is not null)
                await twitchEventSub.UnregisterAsync(string.Join(',', twitch.SubscriptionIds));
            if (transaction is not null) await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<SubscriptionResponse?> UpdateSubscription(ulong guildId, int id, SubscriptionRequest request)
    {
        var rssUrl = request.Url;
        if (request.SubscriptionType == SubscriptionType.Youtube)
        {
            var channelId = GetYoutubeChannelId(request.Url) ?? throw new InvalidChannelURLException("Invalid youtube channel ID");
            rssUrl = $"https://www.youtube.com/feeds/videos.xml?channel_id={channelId}";
        }

        using var context = await contextFactory.CreateDbContextAsync();
        await using var transaction = context.Database.IsRelational()
            ? await context.Database.BeginTransactionAsync()
            : null;

        int length = 0;
        try
        {
            var subscription = await context.Subscriptions.Where(x => x.Id == id && x.GuildId == guildId)
                .Include(s => s.Feed)
                .FirstOrDefaultAsync();

            if (subscription == null) return null;

            if (request.SubscriptionType != SubscriptionType.Twitch && rssUrl != subscription.Feed?.Url)
            {
                subscription.Feed = await FeedRepository.GetOrCreateFeedAsync(context, rssUrl!);
            }

            if (request.SubscriptionType == SubscriptionType.Twitch && !string.IsNullOrEmpty(request.Url))
            {
                var login = TwitchLoginFromUrl(request.Url);
                if (login is not null && !string.Equals(login, subscription.TwitchLogin, StringComparison.OrdinalIgnoreCase))
                {
                    await twitchEventSub.UnregisterAsync(subscription.TwitchEventSubIds);
                    var twitch = await twitchEventSub.RegisterAsync(request.Url);
                    subscription.TwitchLogin = twitch.Login;
                    subscription.TwitchBroadcasterId = twitch.BroadcasterId;
                    subscription.TwitchEventSubIds = string.Join(',', twitch.SubscriptionIds);
                }
            }

            var webhookChanged = !string.IsNullOrEmpty(request.WebhookUrl) && request.WebhookUrl != subscription.WebhookUrl;
            if (webhookChanged)
            {
                var (webhookChannelId, webhookUrl) = await ValidateWebhook(request.WebhookUrl, guildId);
                subscription.WebhookUrl = webhookUrl;
                subscription.WebhookChannel = webhookChannelId;
            }

            var events = await context.Events
                .Where(x => x.SubscriptionId == id)
                    .Include(x => x.Message)
                        .ThenInclude(x => x.Embeds)
                            .ThenInclude(x => x.Fields)
                .ToDictionaryAsync(k => k.Id);

            foreach (var eventRequest in request.Events)
            {
                _ = events.TryGetValue(eventRequest.Value.Id ?? 0, out var currentEvent);

                if (currentEvent == null)
                {
                    currentEvent = new Event
                    {
                        Type = eventRequest.Value.EventType,
                        Subscription = subscription,
                        Message = new Message { Content = "", Embeds = [] }
                    };
                    context.Events.Add(currentEvent);
                }

                currentEvent.Message ??= new Message { Content = "", Embeds = [] };
                currentEvent.Message.Embeds ??= [];

                var incomingMessage = eventRequest.Value.Message;
                currentEvent.Message.Content = incomingMessage.Content;
                currentEvent.Message.WebhookAvatar = incomingMessage.Avatar;
                currentEvent.Message.WebhookUsername = incomingMessage.Username;

                var notFoundEmbeds = currentEvent.Message.Embeds.Where(embed => !incomingMessage.Embeds.Any(req => req.Id == embed.Id)).ToList();
                length += eventRequest.Value.Message.Content?.Length ?? 0;

                foreach (var incomingEmbed in incomingMessage.Embeds)
                {
                    var currentEmbed = currentEvent.Message.Embeds.Find(embed => embed.Id == incomingEmbed.Id);
                    if (currentEmbed is null)
                    {
                        currentEmbed = new Embed
                        {
                            Index = incomingEmbed.Index,
                            Author = incomingEmbed.Author,
                            AuthorUrl = incomingEmbed.AuthorUrl,
                            AuthorIcon = incomingEmbed.AuthorIcon,
                            Title = incomingEmbed.Title,
                            TitleUrl = incomingEmbed.TitleUrl,
                            Description = incomingEmbed.Description,
                            Image = incomingEmbed.Image,
                            Thumbnail = incomingEmbed.Thumbnail,
                            Color = incomingEmbed.Color,
                            Footer = incomingEmbed.Footer,
                            FooterIcon = incomingEmbed.FooterIcon,
                            AddTimestamp = incomingEmbed.AddTimestamp,
                            Message = currentEvent.Message,
                            Fields = []
                        };
                        context.Embeds.Add(currentEmbed);
                        length += incomingEmbed.Length;
                    }
                    else
                    {
                        currentEmbed.Author = incomingEmbed.Author;
                        currentEmbed.AuthorUrl = incomingEmbed.AuthorUrl;
                        currentEmbed.AuthorIcon = incomingEmbed.AuthorIcon;
                        currentEmbed.Title = incomingEmbed.Title;
                        currentEmbed.TitleUrl = incomingEmbed.TitleUrl;
                        currentEmbed.Description = incomingEmbed.Description;
                        currentEmbed.Image = incomingEmbed.Image;
                        currentEmbed.Thumbnail = incomingEmbed.Thumbnail;
                        currentEmbed.Color = incomingEmbed.Color;
                        currentEmbed.Footer = incomingEmbed.Footer;
                        currentEmbed.FooterIcon = incomingEmbed.FooterIcon;
                        currentEmbed.AddTimestamp = incomingEmbed.AddTimestamp;
                        currentEmbed.Index = incomingEmbed.Index;
                        currentEmbed.Fields ??= [];
                        length += SubscriptionMapper.GetEmbedLength(currentEmbed);
                    }

                    var notFoundFields = (currentEmbed.Fields ?? []).Where(field => !incomingEmbed.Fields.Any(req => req.Id == field.Id)).ToList();

                    foreach (var incomingEmbedField in incomingEmbed.Fields)
                    {
                        var currentEmbedField = (currentEmbed.Fields ?? []).Find(embed => embed.Id == incomingEmbedField.Id);

                        if (currentEmbedField is null)
                        {
                            context.EmbedFields.Add(new EmbedField
                            {
                                Index = incomingEmbedField.Index,
                                Name = incomingEmbedField.Name,
                                Value = incomingEmbedField.Value,
                                Inline = incomingEmbedField.Inline,
                                Embed = currentEmbed
                            });
                        }
                        else
                        {
                            currentEmbedField.Index = incomingEmbedField.Index;
                            currentEmbedField.Name = incomingEmbedField.Name;
                            currentEmbedField.Value = incomingEmbedField.Value;
                            currentEmbedField.Inline = incomingEmbedField.Inline;
                            length += currentEmbedField.Name.Length + currentEmbedField.Value.Length;
                        }
                    }

                    foreach (var notFoundField in notFoundFields)
                    {
                        context.EmbedFields.Remove(notFoundField);
                    }
                }

                foreach (var notFoundEmbed in notFoundEmbeds)
                {
                    context.Embeds.Remove(notFoundEmbed);
                }
            }

            if (length > 6000)
            {
                throw new EmbedTooLongException("The embeds and content length cannot be longer than 6k characters");
            }

            await context.SaveChangesAsync();
            if (transaction is not null) await transaction.CommitAsync();

            var updated = await context.Subscriptions
                .Where(x => x.Id == id)
                .Include(s => s.Feed)
                .Include(s => s.Events)
                    .ThenInclude(e => e.Message)
                        .ThenInclude(m => m.Embeds)
                            .ThenInclude(e => e.Fields)
                .SingleAsync();

            return SubscriptionMapper.ToContract(updated);
        }
        catch
        {
            if (transaction is not null) await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<GuildSubscriptionsResponse> GetSubscriptions(ulong guildId, SubscriptionType? provider, bool withCounts = false)
    {
        using var ctx = await contextFactory.CreateDbContextAsync();
        var totalCount = withCounts
            ? await ctx.Subscriptions.CountAsync(s => s.GuildId == guildId)
            : 0;

        var subscriptions = (await ctx.Subscriptions
            .Where(subscription => subscription.GuildId == guildId && (provider == null || subscription.SubscriptionType == provider))
            .Include(s => s.Feed)
            .Include(x => x.Events)
                .ThenInclude(e => e.Message)
                    .ThenInclude(m => m.Embeds)
                        .ThenInclude(e => e.Fields)
            .ToListAsync()).Select(SubscriptionMapper.ToContract).ToList();

        return new GuildSubscriptionsResponse
        {
            Count = withCounts ? totalCount : subscriptions.Count,
            Subscriptions = subscriptions
        };
    }

    public async Task<bool> DeleteSubscription(ulong guildId, int id)
    {
        using var ctx = await contextFactory.CreateDbContextAsync();
        var subscription = await ctx.Subscriptions
            .Include(s => s.Feed)
            .FirstOrDefaultAsync(s => s.Id == id && s.GuildId == guildId);
        if (subscription is null) return false;

        if (subscription.SubscriptionType == SubscriptionType.Twitch)
            await twitchEventSub.UnregisterAsync(subscription.TwitchEventSubIds);

        var feedId = subscription.FeedId;
        ctx.Subscriptions.Remove(subscription);
        await ctx.SaveChangesAsync();

        if (feedId is int fid)
        {
            var stillEnabled = await ctx.Subscriptions.AnyAsync(s => s.FeedId == fid && !s.Disabled);
            if (!stillEnabled)
            {
                var feed = await ctx.Feeds.FirstOrDefaultAsync(f => f.Id == fid);
                if (feed is not null)
                {
                    feed.Disabled = true;
                    await ctx.SaveChangesAsync();
                }
            }
        }

        return true;
    }

    public async Task DisableSubscription(int subscriptionId, string reason)
    {
        using var ctx = await contextFactory.CreateDbContextAsync();
        var subscription = await ctx.Subscriptions.Include(s => s.Feed).FirstOrDefaultAsync(s => s.Id == subscriptionId);
        if (subscription is null) return;
        subscription.Disabled = true;
        subscription.DisabledReason = reason;
        await ctx.SaveChangesAsync();

        if (subscription.FeedId is int fid)
        {
            var stillEnabled = await ctx.Subscriptions.AnyAsync(s => s.FeedId == fid && !s.Disabled);
            if (!stillEnabled && subscription.Feed is not null)
            {
                subscription.Feed.Disabled = true;
                await ctx.SaveChangesAsync();
            }
        }
    }

    public async Task<IReadOnlyList<Subscription>> GetEnabledTwitchSubscriptions(string broadcasterId, CancellationToken cancellationToken = default)
    {
        using var ctx = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await ctx.Subscriptions
            .Where(s => !s.Disabled && s.SubscriptionType == SubscriptionType.Twitch && s.TwitchBroadcasterId == broadcasterId)
            .Include(s => s.Events)
                .ThenInclude(e => e.Message)
                    .ThenInclude(m => m.Embeds)
                        .ThenInclude(e => e.Fields)
            .ToListAsync(cancellationToken);
    }

    private async Task<(ulong ChannelId, string Url)> ValidateWebhook(string? webhookUrl, ulong guildId)
    {
        if (string.IsNullOrEmpty(webhookUrl)) throw new ValidationException("Invalid Discord webhook URL");
        var res = await httpClientFactory.CreateClient().GetAsync(webhookUrl);
        if (!res.IsSuccessStatusCode) throw new ValidationException("Invalid Discord webhook URL");
        var webhookData = await res.Content.ReadFromJsonAsync<WebhookInfo>(DiscordJson.Options)
            ?? throw new ValidationException("Invalid Discord webhook response");
        if (string.IsNullOrEmpty(webhookData.ChannelId) || !ulong.TryParse(webhookData.ChannelId, out var webhookChannelId))
            throw new ValidationException("Webhook Channel ID must exist!");
        if (webhookData.GuildId is not null && webhookData.GuildId != guildId.ToString())
            throw new ValidationException("Webhook does not belong to this guild");
        return (webhookChannelId, webhookUrl);
    }

    private static Message BuildMessage(MessageRequest incoming)
    {
        var message = new Message
        {
            Content = incoming.Content,
            WebhookUsername = incoming.Username,
            WebhookAvatar = incoming.Avatar,
            Embeds = []
        };

        foreach (var embedRequest in incoming.Embeds)
        {
            var embed = new Embed
            {
                Index = embedRequest.Index,
                Description = embedRequest.Description,
                TitleUrl = embedRequest.TitleUrl,
                Title = embedRequest.Title,
                Color = embedRequest.Color,
                Image = embedRequest.Image,
                Author = embedRequest.Author,
                AuthorUrl = embedRequest.AuthorUrl,
                AuthorIcon = embedRequest.AuthorIcon,
                Thumbnail = embedRequest.Thumbnail,
                Footer = embedRequest.Footer,
                FooterIcon = embedRequest.FooterIcon,
                AddTimestamp = embedRequest.AddTimestamp,
                Message = message,
                Fields = []
            };
            foreach (var fieldRequest in embedRequest.Fields)
            {
                embed.Fields.Add(new EmbedField
                {
                    Index = fieldRequest.Index,
                    Name = fieldRequest.Name,
                    Value = fieldRequest.Value,
                    Inline = fieldRequest.Inline,
                    Embed = embed
                });
            }
            message.Embeds.Add(embed);
        }

        return message;
    }

    private static string? GetYoutubeChannelId(string url)
    {
        var match = YoutubeChannelRegex().Match(url);
        return match.Success ? match.Groups[1].Value : null;
    }

    public static string? TwitchLoginFromUrl(string url)
    {
        var match = TwitchChannelRegex().Match(url);
        return match.Success ? match.Groups[1].Value.ToLowerInvariant() : null;
    }

    [GeneratedRegex(@"https?:\/\/(?:www\.)?youtube\.com\/channel\/([a-zA-Z0-9_-]+)")]
    private static partial Regex YoutubeChannelRegex();

    [GeneratedRegex(@"https?:\/\/(?:www\.)?twitch\.tv\/([a-zA-Z0-9_]{4,25})", RegexOptions.IgnoreCase)]
    private static partial Regex TwitchChannelRegex();
}
