
using Hookio.Feeds.Interfaces;

namespace Hookio.Feeds
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public class RssCleanupService(IFeedsCacheService feedsCache) : IHostedService
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
        private CancellationTokenSource _cancellationTokenSource;
        private Task _task;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _task = Task.Factory.StartNew(
                async () => await CleanupSentFeeds(_cancellationTokenSource.Token),
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

        private async Task CleanupSentFeeds(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var expiredFeeds = await feedsCache.GetExpiredFeeds();
                foreach (var expiredFeed in expiredFeeds)
                {
                    await feedsCache.DeleteFeed((int)expiredFeed);
                }
                await Task.Delay(TimeSpan.FromHours(1), cancellationToken);
            }
        }
    }
}
