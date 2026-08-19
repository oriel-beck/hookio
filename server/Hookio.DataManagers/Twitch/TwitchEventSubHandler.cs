using System.Text.Json;
using Hookio.Contracts;
using Hookio.Database.Interfaces;
using Hookio.DataManagers.Utils;
using Hookio.Discord.Contracts;
using Hookio.Discord.Interfaces;
using Hookio.Enums;
using Hookio.Feeds.Interfaces;
using Hookio.Twitch.Contracts;
using Hookio.Utils;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Hookio.Twitch;

public class TwitchEventSubHandler(
    ILogger<TwitchEventSubHandler> logger,
    ISubscriptionService subscriptionService,
    IDiscordRequestManager discordRequestManager,
    IFeedsCacheService feedsCache,
    IConnectionMultiplexer redis)
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public async Task HandleNotificationAsync(string body, CancellationToken cancellationToken = default)
    {
        var payload = JsonSerializer.Deserialize<TwitchEventSubCallback>(body, JsonOptions);
        if (payload?.Event?.BroadcasterUserId is null) return;

        var broadcasterId = payload.Event.BroadcasterUserId;
        var cache = redis.GetDatabase();
        var liveKey = $"twitch:live:{broadcasterId}";

        EventType? eventType = payload.Subscription?.Type switch
        {
            "stream.online" => EventType.TwitchStreamStarted,
            "stream.offline" => EventType.TwitchStreamEnded,
            "channel.update" => EventType.TwitchStreamUpdated,
            _ => null
        };
        if (eventType is null) return;

        if (eventType == EventType.TwitchStreamStarted)
            await cache.StringSetAsync(liveKey, "1");
        else if (eventType == EventType.TwitchStreamEnded)
            await cache.KeyDeleteAsync(liveKey);
        else if (eventType == EventType.TwitchStreamUpdated)
        {
            var live = await cache.StringGetAsync(liveKey);
            if (live.IsNullOrEmpty) return;
        }

        var subscriptions = await subscriptionService.GetEnabledTwitchSubscriptions(broadcasterId, cancellationToken);
        var templates = new List<TemplateStringResponse>
        {
            new() { Key = "user", Value = payload.Event.BroadcasterUserName ?? payload.Event.BroadcasterUserLogin ?? "" },
            new() { Key = "login", Value = payload.Event.BroadcasterUserLogin ?? "" },
            new() { Key = "title", Value = payload.Event.Title ?? "" },
            new() { Key = "game", Value = payload.Event.CategoryName ?? "" },
            new() { Key = "category", Value = payload.Event.CategoryName ?? "" },
            new() { Key = "url", Value = payload.Event.BroadcasterUserLogin is null ? "" : $"https://www.twitch.tv/{payload.Event.BroadcasterUserLogin}" },
        };
        var handler = new TemplateHandler(templates);

        foreach (var subscription in subscriptions)
        {
            var ev = subscription.Events.FirstOrDefault(e => e.Type == eventType);
            if (ev?.Message is null) continue;

            var messagePayload = new DiscordMessageCreatePayload
            {
                AvatarUrl = handler.Parse(ev.Message.WebhookAvatar),
                Username = handler.Parse(ev.Message.WebhookUsername),
                Content = handler.Parse(ev.Message.Content),
                Embeds = DiscordUtils.ConvertEntityEmbedToDiscordEmbed(ev.Message.Embeds ?? [], handler)
            };

            try
            {
                if (eventType == EventType.TwitchStreamUpdated)
                {
                    var messageId = await feedsCache.GetMessageId(TwitchCacheFeedId(subscription.Id), subscription.Id);
                    if (messageId is null) continue;
                    var updated = await discordRequestManager.UpdateWebhookMessage(messagePayload, messageId.Value, subscription.WebhookUrl);
                    if (updated.UnauthorizedOrMissing)
                        await subscriptionService.DisableSubscription(subscription.Id, $"Discord {updated.StatusCode}");
                    continue;
                }

                var sent = await discordRequestManager.SendWebhookMessage(messagePayload, subscription.WebhookUrl);
                if (sent.UnauthorizedOrMissing)
                {
                    await subscriptionService.DisableSubscription(subscription.Id, $"Discord {sent.StatusCode}");
                    continue;
                }
                if (!sent.Success || sent.Message is null) continue;

                if (eventType == EventType.TwitchStreamStarted)
                {
                    await feedsCache.ResetMessages(TwitchCacheFeedId(subscription.Id));
                    await feedsCache.InsertNewFeed(TwitchCacheFeedId(subscription.Id));
                    await feedsCache.InsertNewMessage(TwitchCacheFeedId(subscription.Id), subscription.Id, sent.Message.Id);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send Twitch Discord notification for subscription {Id}", subscription.Id);
            }
        }
    }

    internal static int TwitchCacheFeedId(int subscriptionId) => -subscriptionId;
}
