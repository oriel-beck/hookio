using Hookio.Shared.Enums;

namespace Hookio.DataManagers.Interfaces
{
    public interface IYouTubeSubscriptionCache
    {
        void Add(YouTubeSubscription subscription);
        YouTubeSubscription? Get(string topicUrl);
        void Update(YouTubeSubscription subscription);
        void Delete(string topicUrl);
    }
}