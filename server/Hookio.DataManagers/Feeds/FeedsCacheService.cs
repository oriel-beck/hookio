using Hookio.Feeds.Interfaces;
using StackExchange.Redis;

namespace Hookio.Feeds;

public class FeedsCacheService(IConnectionMultiplexer connectionMultiplexer) : IFeedsCacheService
{
    private readonly IDatabase _redis = connectionMultiplexer.GetDatabase();

    /// <summary>
    /// Hash `SENT_FEED_{feedId}` maps subscriptionId → messageId.
    /// Sorted set `SENT_FEEDS` scores are Unix seconds.
    /// </summary>
    const string SENT_MESSAGES_KEY = "SENT_FEED";
    const string SENT_FEEDS_KEY = "SENT_FEEDS";

    public async Task InsertNewFeed(int feedId)
    {
        await _redis.SortedSetAddAsync(SENT_FEEDS_KEY, feedId, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
    }

    public async Task ResetMessages(int feedId)
    {
        await _redis.KeyDeleteAsync(FeedMessagesKey(feedId));
    }

    public async Task InsertNewMessage(int feedId, int subscriptionId, ulong messageId)
    {
        await _redis.HashSetAsync(FeedMessagesKey(feedId), subscriptionId.ToString(), messageId.ToString());
    }

    public async Task<ulong?> GetMessageId(int feedId, int subscriptionId)
    {
        var value = await _redis.HashGetAsync(FeedMessagesKey(feedId), subscriptionId.ToString());
        if (value.IsNullOrEmpty) return null;
        return ulong.TryParse(value.ToString(), out var id) ? id : null;
    }

    public async Task<RedisValue[]> GetExpiredFeeds()
    {
        var oneDayAgo = DateTimeOffset.UtcNow.AddDays(-1).ToUnixTimeSeconds();
        return await _redis.SortedSetRangeByScoreAsync(SENT_FEEDS_KEY, double.NegativeInfinity, oneDayAgo);
    }

    public async Task<bool> DeleteFeed(int feedId)
    {
        await _redis.SortedSetRemoveAsync(SENT_FEEDS_KEY, feedId);
        return await _redis.KeyDeleteAsync(FeedMessagesKey(feedId));
    }

    public async Task<RedisValue[]> GetAllMessages(int feedId)
    {
        var entries = await _redis.HashGetAllAsync(FeedMessagesKey(feedId));
        return entries.Select(e => (RedisValue)$"{e.Name}-{e.Value}").ToArray();
    }

    private static string FeedMessagesKey(int feedId) => $"{SENT_MESSAGES_KEY}_{feedId}";
}
