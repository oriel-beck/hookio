using Hookio.DataManagers.Interfaces;
using StackExchange.Redis;

namespace Hookio.DataManagers
{
    /// <summary>
    /// This class sets and get videos keys in Redis for 7d, videos older than 7d will not be proccessed.
    /// Key - video ID 
    /// Value - message ID (sent to discord)
    /// </summary>
    /// <param name="connectionMultiplexer"></param>
    public class VideosCacheManager(IConnectionMultiplexer connectionMultiplexer) : IVideosCacheManager
    {
        private readonly IDatabase _database = connectionMultiplexer.GetDatabase();

        public async Task<ulong?> Get(string key)
        {
            var value = await _database.StringGetAsync($"hookio_videos_cache_{key}");
            if (value.IsNull) return null;
            return (ulong)value;
        }

        public async Task<bool> Set(string key, ulong value) =>
            await _database.StringSetAsync($"hookio_videos_cache_{key}", value, TimeSpan.FromDays(7));
    }
}
