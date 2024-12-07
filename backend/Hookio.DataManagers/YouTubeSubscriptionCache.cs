using Hookio.DataManagers.Interfaces;
using Hookio.Shared.Enums;
using Microsoft.Extensions.Caching.Memory;

namespace Hookio.DataManagers
{
    public class YouTubeSubscription
    {
        public required string TopicUrl { get; set; }
        public YouTubeSubscriptionStatus Status { get; set; } = YouTubeSubscriptionStatus.Pending;
        // TODO: channel data (with expiry)
    }

    /// <summary>
    /// This is caching for 2 things.
    /// 1. Channel data of the YT channel (unused atm)
    /// 2. Subscription data to only "subscribe" to pending subscriptions
    /// </summary>
    /// <param name="cache"></param>
    public class YouTubeSubscriptionCache(IMemoryCache cache) : IYouTubeSubscriptionCache
    {
        private readonly IMemoryCache _cache = cache;
        private const string CacheKey = "Subscriptions";

        public Task AddAsync(YouTubeSubscription subscription)
        {
            var subscriptions = _cache.GetOrCreate(CacheKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
                return new List<YouTubeSubscription>();
            });
            subscriptions?.Add(subscription);
            _cache.Set(CacheKey, subscriptions);
            return Task.CompletedTask;
        }

        public Task<YouTubeSubscription?> GetByTopicUrlAsync(string topicUrl)
        {
            var subscriptions = _cache.GetOrCreate(CacheKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
                return new List<YouTubeSubscription>();
            });
            var subscription = subscriptions?.FirstOrDefault(s => s.TopicUrl == topicUrl);
            return Task.FromResult(subscription);
        }

        public Task<IEnumerable<YouTubeSubscription>?> GetSubscriptionsAsync(YouTubeSubscriptionStatus? status)
        {
            var subscriptions = _cache.GetOrCreate(CacheKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
                return new List<YouTubeSubscription>();
            });
            var activeSubscriptions = subscriptions?.Where(s => status == null || s.Status == status);
            return Task.FromResult(activeSubscriptions);
        }
    }
}
