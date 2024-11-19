using Hookio.Contracts.Discord;
using Hookio.Contracts.Subscription;
using Hookio.Data;
using Hookio.Data.Entities;
using Hookio.DataManagers.Interfaces;
using Hookio.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using System.Text.RegularExpressions;

namespace Hookio.DataManagers
{
    public partial class SubscriptionManager(IDbContextFactory<HookioContext> contextFactory, IHttpClientFactory httpClientFactory) : ISubscriptionManager
    {
        private readonly IDbContextFactory<HookioContext> _contextFactory = contextFactory;
        private readonly HttpClient _httpClient = httpClientFactory.CreateClient("WebhooksCheck");

        [GeneratedRegex(@"^https:\/\/(canary\.|ptb\.|www\.)?discord\.com\/api\/webhooks\/(?<webhookId>\d+){17,19}\/(?<webhookToken>[A-Za-z0-9_-]+)$", RegexOptions.IgnoreCase)]
        private static partial Regex WebhookRegex();

        public async Task<SubscriptionResponse?> Create(string guildId, SubscriptionRequest request, CancellationToken cancellationToken)
        {
            var validWebhook = TestAndParseWebhookUrl(request.WebhookUrl);
            if (!validWebhook) throw new ValidationException("Invalid webhook URL");

            var webhook = await _httpClient.GetFromJsonAsync<DiscordWebhook>(request.WebhookUrl, cancellationToken) ?? throw new ValidationException("Invalid webhook");

            using var ctx = await _contextFactory.CreateDbContextAsync(cancellationToken);

            Subscription res;
            await ctx.AddAsync(res = new()
            {
                GuildId = guildId,
                SubscriptionType = request.SubscriptionType,
                WebhookUrl = webhook.Url,
                ChannelId = webhook.ChannelId,
            }, cancellationToken);

            // generate Id for subscription
            await ctx.SaveChangesAsync(cancellationToken);

            // generate initial messages
            res.Messages = GenerateInitialMessages(res);

            // save everything
            await ctx.SaveChangesAsync(cancellationToken);

            return Contractor.ToContract(res);
        }

        public async Task<IEnumerable<SubscriptionResponse?>> Get(string guildId, SubscriptionFilter filter, CancellationToken cancellationToken)
        {
            using var ctx = await _contextFactory.CreateDbContextAsync(cancellationToken);

            IQueryable<Subscription> query = ctx.Subscriptions;

            if (filter.IncludeMessages)
                query = query.Include(x => x.Messages);

            var res = await query
                .Where(x => x.GuildId == guildId)
                .ToListAsync(cancellationToken);


            return res.Select(Contractor.ToContract);
        }

        public async Task<SubscriptionResponse?> Get(string guildId, int subscriptionId, CancellationToken cancellationToken)
        {
            using var ctx = await _contextFactory.CreateDbContextAsync(cancellationToken);

            var res = await ctx.Subscriptions
                .Include(x => x.Messages)
                .FirstOrDefaultAsync(x => x.GuildId == guildId && x.Id == subscriptionId, cancellationToken);

            return Contractor.ToContract(res);
        }

        public async Task<SubscriptionResponse?> Patch(string guildId, int subscriptionId, SubscriptionPatch patch, CancellationToken cancellationToken)
        {
            using var ctx = await _contextFactory.CreateDbContextAsync(cancellationToken);
            await ctx.Database.BeginTransactionAsync(cancellationToken);

            var res = await ctx.Subscriptions
                .Include(x => x.Messages)
                .FirstOrDefaultAsync(x => x.GuildId == guildId && x.Id == subscriptionId, cancellationToken);

            if (res == null) return null;

            if (patch.WebhookUrl != null)
            {
                if (!TestAndParseWebhookUrl(patch.WebhookUrl)) throw new ValidationException("Invalid webhook URL");
                var webhook = await _httpClient.GetFromJsonAsync<DiscordWebhook>(patch.WebhookUrl, cancellationToken) ?? throw new ValidationException("Invalid webhook");
                res.WebhookUrl = webhook.Url;
                res.ChannelId = webhook.ChannelId;
            }

            if (patch.Messages != null)
            {
                if (patch.Messages.Any(m => !m.IsValid))
                {
                    await ctx.Database.RollbackTransactionAsync(cancellationToken);
                    // TODO: better exceptions (which message failed) and logging
                    throw new ValidationException("Message is invalid");
                }
                Parallel.ForEach(patch.Messages, msg =>
                {
                    var origin = res.Messages?.FirstOrDefault(m => m.Id == msg.Id);
                    if (origin == null) return;
                    origin.Content = msg.Content;
                    origin.Embeds = msg.Embeds;

                    // this can set things that if set manually can do stupid shit, but they can't break the app.
                    // If a user decides to send a custom value here to break their shit, they only break their shit.
                    origin.Type = msg.Type;
                    origin.Action = msg.Action;
                });
            }

            if (patch.Source != null)
            {
                // TODO: implement
            }

            await ctx.Database.CommitTransactionAsync(cancellationToken);
            await ctx.SaveChangesAsync(cancellationToken);

            return Contractor.ToContract(res);
        }

        private static List<Message> GenerateInitialMessages(Subscription subscription)
        {
            return subscription.SubscriptionType switch
            {
                SubscriptionType.YouTube => new()
                    {
                        { new() { Action = null, Type = MessageType.YouTubeVideoCreated, SubscriptionId = subscription.Id } },
                        { new() { Action = null, Type = MessageType.YouTubeVideoUpdated, SubscriptionId = subscription.Id } },
                        { new() { Action = null, Type = MessageType.YouTubeVideoDeleted, SubscriptionId = subscription.Id } }
                    },
                SubscriptionType.Twitch => new()
                    {
                        { new() { Action = null, Type = MessageType.TwitchStreamStarted, SubscriptionId = subscription.Id } },
                        { new() { Action = null, Type = MessageType.TwitchStreamUpdated, SubscriptionId = subscription.Id } },
                        { new() { Action = null, Type = MessageType.TwitchStreamEnded, SubscriptionId = subscription.Id } }
                    },
                _ => [],
            };
        }

        private static bool TestAndParseWebhookUrl(string url)
        {
            Match match = WebhookRegex().Match(url);
            if (!match.Success)
            {
                return false;
            }

            return true;
        }
    }
}
