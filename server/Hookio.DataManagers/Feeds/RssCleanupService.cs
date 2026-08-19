using Hookio.Feeds.Interfaces;
using Microsoft.Extensions.Hosting;

namespace Hookio.Feeds;

public class RssCleanupService(IFeedsCacheService feedsCache) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var expiredFeeds = await feedsCache.GetExpiredFeeds();
                foreach (var expiredFeed in expiredFeeds)
                {
                    await feedsCache.DeleteFeed((int)expiredFeed);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch
            {
                // next hour
            }

            try
            {
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }
}
