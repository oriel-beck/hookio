using Hookio.Database.Entities;
using Hookio.Database.Interfaces;
using Hookio.Discord.Contracts;
using Hookio.Discord.Interfaces;
using Hookio.Enunms;
using Hookio.Feeds;
using Hookio.Feeds.Interfaces;
using Hookio.Utils;
using StackExchange.Redis;
using System.Collections.Concurrent;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
internal class RssWatcherService(ILogger<RssWatcherService> logger, IDataManager dataManager, IHttpClientFactory httpClientFactory, IDiscordRequestManager discordRequestManager, IFeedsCacheService feedsCacheService) : IHostedService
{
    private readonly DomainRateLimiter _rateLimiter = new(httpClientFactory, logger, discordRequestManager, feedsCacheService);
    private CancellationTokenSource _cancellationTokenSource;
    private Task _task;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _cancellationTokenSource = new CancellationTokenSource();
        _task = Task.Factory.StartNew(
            async () => await WatchFeeds(_cancellationTokenSource.Token),
            _cancellationTokenSource.Token,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Default
        );

        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _cancellationTokenSource.Cancel();
        await _task;
    }
    /*
     * It seems like `pubDate` and `published` (and `updated`) can appear.
        If a feed does not have `pubDate`/`published`, use `updated` as the saved published date.
     * It seems like `guid` and `id` can appear as identifiers
     * These are required. DO NOT save/publish a feed if:
        - missing `pubDate`, `published` and `updated`
        - missing `guid` and `id`
     */
    private async Task WatchFeeds(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            List<Task<Feed?>> tasks = [];
            var feeds = await dataManager.GetAllFeeds(cancellationToken);
            foreach (var feed in feeds)
            {
                tasks.Add(_rateLimiter.SendRequestAsync(feed, cancellationToken));
            }
            var results = await Task.WhenAll(tasks);
            foreach (var result in results)
            {
                if (result == null) continue;
                try
                {
                    await dataManager.UpdateFeed(result.Id, result);
                }
                catch (Exception ex)
                {
                    logger.LogError("Failed to update feed data for {FeedId}\nError: {ErrorMessage}\nStack: {ErrorStack}", result.Id, ex.Message, ex.StackTrace);
                }
            }
            await Task.Delay(TimeSpan.FromMinutes(15), cancellationToken);
        }
    }

    internal class DomainRateLimiter(IHttpClientFactory httpClientFactory, ILogger<RssWatcherService> logger, IDiscordRequestManager discordRequestManager, IFeedsCacheService feedsCacheService)
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

                // Read the rate limit headers and update the rate limit info
                if (response.Headers.TryGetValues("x-ratelimit-remaining", out var remainingValues) &&
                    response.Headers.TryGetValues("x-ratelimit-reset", out var resetValues) &&
                    int.TryParse(remainingValues.First(), out var remaining) &&
                    double.TryParse(resetValues.First(), out var resetInSeconds) &&
                    resetInSeconds > 0)
                {
                    DateTimeOffset resetTime = DateTimeOffset.UtcNow.AddSeconds(resetInSeconds);

                    if (remaining <= 0)
                    {
                        // Update the reset time if the limit is reached
                        rateLimitInfo.resetTime = resetTime;
                    }
                }
                else
                {
                    // Default to 1 seconds if no rate limit headers are present
                    rateLimitInfo.resetTime = DateTimeOffset.UtcNow.AddSeconds(1);
                }

                // TODO: add special additional template strings to YT feeds
                var templateStrings = await FeedUtils.Parse(response);
                var xmlDetails = templateStrings.Item2;
                if (xmlDetails.Id == null || (xmlDetails.Updated == null && xmlDetails.Published == null))
                {
                    logger.LogError("Error: Missing feed information!\nFailed to parse RSS feed {Url} with ID {Id}, XmlDetails: \nID: {Id}\nPublished: {Published}\nUpdated: {Updated}", [
                        feed.Url,
                        feed.Id,
                        xmlDetails.Id,
                        xmlDetails.Published,
                        xmlDetails.Published
                        ]);
                    return null;
                }

                if (feed.LastId == xmlDetails.Id && feed.LastPublishedAt - xmlDetails.Published > TimeSpan.FromDays(1))
                {
                    logger.LogInformation("Skipped updating feed {Id}, more than 1d passed since last published", feed.Id);
                    return null;
                }

                var enabledSubscriptions = feed.Subscriptions.Where(s => !s.Disabled);
                if (!enabledSubscriptions.Any())
                {
                    logger.LogInformation("Feed {Id} to {Url} has no enabled subscriptions, disabling", feed.Id, feed.Url);
                    // TODO: disable/delete feed
                    return null;
                }

                var templateHandler = new TemplateHandler(templateStrings.Item1);
                
                // prepare caching the feed
                await feedsCacheService.InsertNewFeed(feed.Id);

                var sentMessages = (await feedsCacheService.GetAllMessages(feed.Id)).ToDictionary(rv => rv.ToString().Split("-").First(), rv => rv.ToString().Split('-').Last());

                foreach (var subscription in enabledSubscriptions)
                {
                    var published = xmlDetails.Published ?? xmlDetails.Updated;
                    var updated = xmlDetails.Updated ?? xmlDetails.Published;
                    // if the feed id is not the same as the xmlDetails id, publish the feed as a new feed and update the `LastId` and `LastPublishedAt` values
                    if (feed.LastId != xmlDetails.Id)
                    {
                        var ev = subscription.Events.FirstOrDefault((e) => e.Type == EventType.NewFeed);
                        if (ev == null) continue;
                        // do not await, simply send it to be queued
                        SendSubscription(feed, subscription, templateHandler);
                    }
                    else if (feed.LastPublishedAt != published || feed.LastPublishedAt != updated)
                    {
                        var exists = ulong.TryParse(sentMessages[feed.Id.ToString()], out var messageId);
                        // skip any feeds that didn't send a message or failed to send a message
                        if (!exists) continue;
                        // do not await, simply send it to be queued
                        UpdateSubscription(subscription, templateHandler, messageId);
                    }
                }

                feed.LastPublishedAt = DateTime.UtcNow;
                feed.LastId = xmlDetails.Id;
                return feed;
            }
            finally
            {
                // Release the semaphore based on the reset time
                if (DateTimeOffset.UtcNow >= rateLimitInfo.resetTime)
                {
                    rateLimitInfo.semaphore.Release();
                }
                else
                {
                    // Schedule the release of the semaphore after the reset time has passed
                    _ = ReleaseSemaphoreAfterDelay(rateLimitInfo.semaphore, rateLimitInfo.resetTime, cancellationToken);
                }
            }
        }

        private static async Task ReleaseSemaphoreAfterDelay(SemaphoreSlim semaphore, DateTimeOffset resetTime, CancellationToken cancellationToken)
        {
            TimeSpan delay = resetTime - DateTimeOffset.UtcNow;
            // TODO: if delay is too long cancel all future requests to this domain.
            await Task.Delay(delay, cancellationToken);
            semaphore.Release();
        }

        private async Task SendSubscription(Feed feed, Subscription subscription, TemplateHandler templateHandler)
        {
            var ev = subscription.Events.FirstOrDefault((e) => e.Type == EventType.NewFeed);
            if (ev == null) return;
            var messagePayload = new DiscordMessageCreatePayload
            {
                Avatar = templateHandler.Parse(ev.Message.WebhookAvatar),
                Username = templateHandler.Parse(ev.Message.WebhookUsername),
                Content = templateHandler.Parse(ev.Message?.Content),
                Embeds = DiscordUtils.ConvertEntityEmbedToDiscordEmbed(ev.Message?.Embeds ?? [], templateHandler)
            };
            try
            {
                var message = await discordRequestManager.SendWebhookMessage(messagePayload, subscription.WebhookUrl) ?? throw new Exception("Failed to send webhook, discord did not return a message");
                await feedsCacheService.InsertNewMessage(feed.Id, message.Id);
            }
            catch (Exception ex)
            {
                logger.LogError("Failed to send webhook message for subscription {Id}\nError: {ErrorMessage}\nStack: {ErrorStack}", subscription.Id, ex.Message, ex.StackTrace);
                // TODO: disable the subscription
            }
        }

        private async Task UpdateSubscription(Subscription subscription, TemplateHandler templateHandler, ulong messageId)
        {
            var ev = subscription.Events.FirstOrDefault((e) => e.Type == EventType.UpdatedFeed);
            if (ev == null) return;
            var messagePayload = new DiscordMessageCreatePayload
            {
                Avatar = templateHandler.Parse(ev.Message.WebhookAvatar),
                Username = templateHandler.Parse(ev.Message.WebhookUsername),
                Content = templateHandler.Parse(ev.Message?.Content),
                Embeds = DiscordUtils.ConvertEntityEmbedToDiscordEmbed(ev.Message?.Embeds ?? [], templateHandler)
            };
            try
            {
                await discordRequestManager.UpdateWebhookMessage(messagePayload, messageId, subscription.WebhookUrl);
            }
            catch (Exception ex)
            {
                logger.LogError("Failed to update webhook message for subscription {Id}\nError: {ErrorMessage}\nStack: {ErrorStack}", subscription.Id, ex.Message, ex.StackTrace);
                // TODO: disable the subscription
            }
        }
    }
}