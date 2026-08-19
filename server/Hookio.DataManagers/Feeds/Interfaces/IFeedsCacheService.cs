using StackExchange.Redis;

namespace Hookio.Feeds.Interfaces
{
    public interface IFeedsCacheService
    {
        Task<RedisValue[]> GetExpiredFeeds();
        Task InsertNewFeed(int feedId);
        Task ResetMessages(int feedId);
        Task InsertNewMessage(int feedId, int subscriptionId, ulong messageId);
        Task<ulong?> GetMessageId(int feedId, int subscriptionId);
        Task<bool> DeleteFeed(int feedId);
        Task<RedisValue[]> GetAllMessages(int feedId);
    }
}
