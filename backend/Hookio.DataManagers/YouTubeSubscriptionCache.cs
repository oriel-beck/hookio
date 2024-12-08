using Google.Apis.YouTube.v3.Data;
using Hookio.DataManagers.Interfaces;
using Hookio.Shared.Enums;
using Microsoft.Extensions.Caching.Memory;

namespace Hookio.DataManagers
{
    public class YouTubeSubscription
    {
        public YouTubeSubscriptionStatus Status { get; set; } = YouTubeSubscriptionStatus.Pending;
        public string? VerifyToken { get; set; }
        public required string ChannelId { get; set; }
        public Channel? Channel { get; set; }
        public Video? LatestVideo { get; set; }
    }

    /// <summary>
    /// Manages YouTube subscriptions with individual expiration logic.
    /// </summary>
    public class YouTubeSubscriptionCache(IMemoryCache cache) : IYouTubeSubscriptionCache
    {
        private readonly IMemoryCache _cache = cache;

        private static string GetCacheKey(string channelId) => $"Subscription_{channelId}";

        public void Add(YouTubeSubscription subscription)
        {
            var cacheKey = GetCacheKey(subscription.ChannelId);
            _cache.Set(cacheKey, subscription, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
            });
        }

        public void Delete(string topicUrl)
        {
            _cache.Remove(GetCacheKey(topicUrl));
        }

        public YouTubeSubscription? Get(string channelId)
        {
            var cacheKey = GetCacheKey(channelId);
            _cache.TryGetValue(cacheKey, out YouTubeSubscription? subscription);
            return subscription;
        }

        public void Update(YouTubeSubscription subscription)
        {
            var cacheKey = GetCacheKey(subscription.ChannelId);
            _cache.Set(cacheKey, subscription, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
            });
        }
    }
}
