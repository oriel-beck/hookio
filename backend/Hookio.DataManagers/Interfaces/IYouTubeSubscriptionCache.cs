using Hookio.Shared.Enums;

namespace Hookio.DataManagers.Interfaces
{
    public interface IYouTubeSubscriptionCache
    {
        Task AddAsync(YouTubeSubscription subscription);
        Task<YouTubeSubscription?> GetByTopicUrlAsync(string topicUrl);
        Task<IEnumerable<YouTubeSubscription>?> GetSubscriptionsAsync(YouTubeSubscriptionStatus? status);
    }
}