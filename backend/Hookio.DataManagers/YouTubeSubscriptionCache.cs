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
    /// Manages YouTube subscriptions cache with individual expiration logic.<br/>
    /// This class saves a cache of subscriptions that are being subscribed to or cache of notifications that were sent<br/>
    /// It saves the channel data (so it can be reused without fetching it again) and latest video data (in case it gets updated)
    /// </summary>
    public class YouTubeSubscriptionCache(IMemoryCache cache) : IYouTubeSubscriptionCache
    {
        private readonly IMemoryCache _cache = cache;

        private static string GetCacheKey(string channelId) => $"Subscription_{channelId}";

        /// <summary>
        /// Adds a YouTube subscriptions to the cache<br/>
        /// Used when subscribing to a new subscription in order to validate the callback
        /// </summary>
        /// <param name="subscription"></param>
        public void Add(YouTubeSubscription subscription)
        {
            var cacheKey = GetCacheKey(subscription.ChannelId);
            _cache.Set(cacheKey, subscription, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
            });
        }

        /// <summary>
        /// Removes a YouTube subscriptions from the cache
        /// </summary>
        /// <param name="topicUrl"></param>
        public void Delete(string topicUrl)
        {
            _cache.Remove(GetCacheKey(topicUrl));
        }

        /// <summary>
        /// Gets a YouTube subscription from the cache
        /// </summary>
        /// <param name="channelId"></param>
        /// <returns></returns>
        public YouTubeSubscription? Get(string channelId)
        {
            var cacheKey = GetCacheKey(channelId);
            _cache.TryGetValue(cacheKey, out YouTubeSubscription? subscription);
            return subscription;
        }

        /// <summary>
        /// Replaces or creates an instance of a subscription in the cache and extends its expiration
        /// </summary>
        /// <param name="subscription"></param>
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
