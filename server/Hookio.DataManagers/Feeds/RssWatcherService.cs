using Hookio.Database.Entities;
using Hookio.Database.Interfaces;
using Hookio.DataManagers.Utils;
using Hookio.Discord.Contracts;
using Hookio.Discord.Interfaces;
using Hookio.Enums;
using Hookio.Feeds.Interfaces;
using Hookio.Utils;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Hookio.Feeds;

public class RssWatcherService(
    ILogger<RssWatcherService> logger,
    IFeedRepository feedRepository,
    ISubscriptionService subscriptionService,
    IHttpClientFactory httpClientFactory,
    IDiscordRequestManager discordRequestManager,
    IFeedsCacheService feedsCacheService) : BackgroundService
{
    private readonly DomainRateLimiter _rateLimiter = new(httpClientFactory, logger, discordRequestManager, feedsCacheService, subscriptionService);

    /*
     * `pubDate`/`published` and `updated` can appear.
     * If a feed does not have `pubDate`/`published`, use `updated` as the saved published date.
     * `guid` and `id` can appear as identifiers.
     * Do not save/publish a feed if missing id/guid or missing all of pubDate/published/updated.
     */
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var feeds = await feedRepository.GetAllFeeds(stoppingToken);
                var tasks = feeds.Select(feed => _rateLimiter.SendRequestAsync(feed, stoppingToken)).ToList();
                var results = await Task.WhenAll(tasks);
                foreach (var result in results)
                {
                    if (result == null) continue;
                    try
                    {
                        await feedRepository.UpdateFeed(result.Id, result);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError("Failed to update feed data for {FeedId}\nError: {ErrorMessage}\nStack: {ErrorStack}", result.Id, ex.Message, ex.StackTrace);
                    }
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "RSS watch cycle failed");
            }

            try
            {
                await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }

    internal class DomainRateLimiter(IHttpClientFactory httpClientFactory, ILogger<RssWatcherService> logger, IDiscordRequestManager discordRequestManager, IFeedsCacheService feedsCacheService, ISubscriptionService subscriptionService)
    {
        private readonly ConcurrentDictionary<string, (SemaphoreSlim semaphore, DateTimeOffset resetTime)> _rateLimits = new();

        public async Task<Feed?> SendRequestAsync(Feed feed, CancellationToken cancellationToken = default)
        {
            _ = Uri.TryCreate(feed.Url, UriKind.Absolute, out var uri);
            string domainKey = uri!.Host.ToLowerInvariant();
            var rateLimitInfo = _rateLimits.GetOrAdd(domainKey, _ => (new SemaphoreSlim(1, 1), DateTimeOffset.UtcNow));

            await rateLimitInfo.semaphore.WaitAsync(cancellationToken);
            try
            {
                var _httpClient = httpClientFactory.CreateClient();
                HttpResponseMessage response = await _httpClient.GetAsync(uri, cancellationToken);

                if (response.Headers.TryGetValues("x-ratelimit-remaining", out var remainingValues) &&
                    response.Headers.TryGetValues("x-ratelimit-reset-after", out var resetValues) &&
                    int.TryParse(remainingValues.First(), out var remaining) &&
                    double.TryParse(resetValues.First(), out var resetInSeconds) &&
                    resetInSeconds > 0)
                {
                    DateTimeOffset resetTime = DateTimeOffset.UtcNow.AddSeconds(resetInSeconds);
                    if (remaining <= 0)
                    {
                        rateLimitInfo.resetTime = resetTime;
                    }
                }
                else
                {
                    rateLimitInfo.resetTime = DateTimeOffset.UtcNow.AddSeconds(1);
                }

                var templateStrings = await FeedUtils.Parse(response);
                var xmlDetails = templateStrings.Item2;
                if (xmlDetails.Id == null || (xmlDetails.Updated == null && xmlDetails.Published == null))
                {
                    logger.LogError("Error: Missing feed information!\nFailed to parse RSS feed {Url} with ID {Id}, XmlDetails: \nID: {XmlId}\nPublished: {Published}\nUpdated: {Updated}",
                        feed.Url,
                        feed.Id,
                        xmlDetails.Id,
                        xmlDetails.Published,
                        xmlDetails.Updated);
                    return null;
                }

                var published = xmlDetails.Published ?? xmlDetails.Updated;
                var updated = xmlDetails.Updated ?? xmlDetails.Published;
                var entryTime = updated ?? published;

                var isNew = feed.LastId != xmlDetails.Id;
                var isUpdated = !isNew && feed.LastPublishedAt != entryTime;

                if (!isNew && !isUpdated)
                {
                    logger.LogInformation("Skipped feed {Id}, no id/published/updated change", feed.Id);
                    return null;
                }

                var enabledSubscriptions = feed.Subscriptions.Where(s => !s.Disabled).ToList();
                if (enabledSubscriptions.Count == 0)
                {
                    logger.LogInformation("Feed {Id} to {Url} has no enabled subscriptions, disabling", feed.Id, feed.Url);
                    feed.Disabled = true;
                    return feed;
                }

                var templateHandler = new TemplateHandler(templateStrings.Item1);

                if (isNew)
                {
                    await feedsCacheService.ResetMessages(feed.Id);
                    await feedsCacheService.InsertNewFeed(feed.Id);
                }

                foreach (var subscription in enabledSubscriptions)
                {
                    if (isNew)
                    {
                        await SendSubscription(feed, subscription, templateHandler);
                    }
                    else
                    {
                        var messageId = await feedsCacheService.GetMessageId(feed.Id, subscription.Id);
                        if (messageId is null) continue;
                        await UpdateSubscription(subscription, templateHandler, messageId.Value);
                    }
                }

                feed.LastPublishedAt = entryTime;
                feed.LastId = xmlDetails.Id;
                return feed;
            }
            finally
            {
                if (DateTimeOffset.UtcNow >= rateLimitInfo.resetTime)
                {
                    rateLimitInfo.semaphore.Release();
                }
                else
                {
                    _ = ReleaseSemaphoreAfterDelay(rateLimitInfo.semaphore, rateLimitInfo.resetTime, cancellationToken);
                }
            }
        }

        private static async Task ReleaseSemaphoreAfterDelay(SemaphoreSlim semaphore, DateTimeOffset resetTime, CancellationToken cancellationToken)
        {
            TimeSpan delay = resetTime - DateTimeOffset.UtcNow;
            if (delay < TimeSpan.Zero) delay = TimeSpan.Zero;
            await Task.Delay(delay, cancellationToken);
            semaphore.Release();
        }

        private async Task SendSubscription(Feed feed, Subscription subscription, TemplateHandler templateHandler)
        {
            var ev = subscription.Events.FirstOrDefault(e => e.Type == EventType.NewFeed);
            if (ev?.Message == null) return;
            var messagePayload = new DiscordMessageCreatePayload
            {
                AvatarUrl = templateHandler.Parse(ev.Message.WebhookAvatar),
                Username = templateHandler.Parse(ev.Message.WebhookUsername),
                Content = templateHandler.Parse(ev.Message.Content),
                Embeds = DiscordUtils.ConvertEntityEmbedToDiscordEmbed(ev.Message.Embeds ?? [], templateHandler)
            };
            try
            {
                var result = await discordRequestManager.SendWebhookMessage(messagePayload, subscription.WebhookUrl);
                if (result.UnauthorizedOrMissing)
                {
                    await subscriptionService.DisableSubscription(subscription.Id, $"Discord {result.StatusCode}");
                    return;
                }
                if (!result.Success || result.Message is null) throw new Exception("Failed to send webhook, discord did not return a message");
                await feedsCacheService.InsertNewMessage(feed.Id, subscription.Id, result.Message.Id);
            }
            catch (Exception ex)
            {
                logger.LogError("Failed to send webhook message for subscription {Id}\nError: {ErrorMessage}\nStack: {ErrorStack}", subscription.Id, ex.Message, ex.StackTrace);
            }
        }

        private async Task UpdateSubscription(Subscription subscription, TemplateHandler templateHandler, ulong messageId)
        {
            var ev = subscription.Events.FirstOrDefault(e => e.Type == EventType.UpdatedFeed);
            if (ev?.Message == null) return;
            var messagePayload = new DiscordMessageCreatePayload
            {
                AvatarUrl = templateHandler.Parse(ev.Message.WebhookAvatar),
                Username = templateHandler.Parse(ev.Message.WebhookUsername),
                Content = templateHandler.Parse(ev.Message.Content),
                Embeds = DiscordUtils.ConvertEntityEmbedToDiscordEmbed(ev.Message.Embeds ?? [], templateHandler)
            };
            try
            {
                var result = await discordRequestManager.UpdateWebhookMessage(messagePayload, messageId, subscription.WebhookUrl);
                if (result.UnauthorizedOrMissing)
                    await subscriptionService.DisableSubscription(subscription.Id, $"Discord {result.StatusCode}");
            }
            catch (Exception ex)
            {
                logger.LogError("Failed to update webhook message for subscription {Id}\nError: {ErrorMessage}\nStack: {ErrorStack}", subscription.Id, ex.Message, ex.StackTrace);
            }
        }
    }
}
