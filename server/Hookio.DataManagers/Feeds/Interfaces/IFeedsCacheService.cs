using StackExchange.Redis;

namespace Hookio.Feeds.Interfaces
{
    public interface IFeedsCacheService
    {
        public Task<RedisValue[]> GetExpiredFeeds();
        public Task InsertNewFeed(int feedId);
        public Task InsertNewMessage(int feedId, ulong messageId);
        public Task<bool> DeleteFeed(int feedId);
        public Task<RedisValue[]> GetAllMessages(int feedId);
    }
}