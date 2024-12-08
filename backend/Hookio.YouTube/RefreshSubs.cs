using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using Hookio.DataManagers.Interfaces;

namespace Hookio.YouTube
{
    public class RefreshSubs(
        ILogger<RefreshSubs> logger,
        IConnectionMultiplexer connectionMultiplexer,
        IYouTubeManager youTubeManager
        ) : BackgroundService
    {
        private readonly PeriodicTimer _periodicTimer = new(TimeSpan.FromHours(1));
        private readonly ILogger<RefreshSubs> _logger = logger;
        private readonly IDatabase _redis = connectionMultiplexer.GetDatabase();
        private readonly IYouTubeManager _youTubeManager = youTubeManager;

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting YT resubbing service");
            while (
                !cancellationToken.IsCancellationRequested &&
                await _periodicTimer.WaitForNextTickAsync(cancellationToken))
            {
                try
                {
                    // Get anything that's older than (now - 1h)
                    var list = await _redis.SortedSetRangeByScoreAsync(
                        "hookio_yt_subs",
                        start: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - TimeSpan.FromHours(1).TotalMilliseconds);

                    // Each item is a YT channel subscription
                    foreach (var item in list)
                    {
                        // TODO: If the channel has no subscriptions anymore do not resub
                        var res = await _youTubeManager.Subscribe(item.ToString(), cancellationToken);
                        if (res?.StatusCode == System.Net.HttpStatusCode.Accepted)
                        {
                            _logger.LogInformation("Refreshed subscription for {channel_id}", item.ToString());
                        }
                        else
                        {
                            _logger.LogInformation("Failed to resubscribe for {channel_id}", item.ToString());
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("Failed to get list of subscriptions\n{exception}", ex.Message);
                }
            }
        }
    }
}
