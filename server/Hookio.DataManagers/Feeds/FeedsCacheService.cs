
using Hookio.Feeds.Interfaces;
using StackExchange.Redis;

namespace Hookio.Feeds;
public class FeedsCacheService(IConnectionMultiplexer connectionMultiplexer) : IFeedsCacheService
{
    private readonly IDatabase _redis = connectionMultiplexer.GetDatabase();
    /// <summary>
    /// The sent feed key for redis (set), used as `SENT_FEED_{feedId}`, contains `{subscriptionId}-{messageId}
    /// Publish: loop through every enabled subscription in the feed, send it, then store it
    /// Update: loop through every enabled subscription in the feed, get the messageId by the subscriptionId, then edit it
    /// Cleanup: delete the set.
    /// </summary>
    const string SENT_MESSAGES_KEY = "SENT_FEED";
    /// <summary>
    /// The sent feeds key for redis (sorted set), key is feedId, score is TIMESTAMP, do not edit feeds that are over 1d old.
    /// Publish: Store the feed ID after publishing the messages.
    /// Update: do nothing
    /// Cleanup: Get all feeds that are over 1d old, then delete `{SENT_MESSAGES_KEY}_{feedId}` set and the entry.
    /// </summary>
    const string SENT_FEEDS_KEY = "SENT_FEEDS";

    public async Task InsertNewFeed(int feedId)
    {
        await _redis.KeyDeleteAsync(FeedMessagesKey(feedId));
        await _redis.SortedSetAddAsync(SENT_FEEDS_KEY, feedId, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
    }
    public async Task InsertNewMessage(int feedId, ulong messageId)
    {
        await _redis.SetAddAsync(FeedMessagesKey(feedId), messageId);
    }

    public async Task<RedisValue[]> GetExpiredFeeds()
    {
        // TODO: test this out
        double oneDayAgo = (DateTime.UtcNow - TimeSpan.FromDays(1)).Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        return await _redis.SortedSetRangeByScoreAsync(SENT_FEEDS_KEY, double.NegativeInfinity, oneDayAgo);
    }

    public async Task<bool> DeleteFeed(int feedId)
    {
        await _redis.SortedSetRemoveAsync(SENT_FEEDS_KEY, feedId);
        return await _redis.KeyDeleteAsync(FeedMessagesKey(feedId));
    }

    public async Task<RedisValue[]> GetAllMessages(int feedId)
    {
        return await _redis.SetMembersAsync(FeedMessagesKey(feedId));
    }

    private static string FeedMessagesKey(int feedId) => $"{SENT_MESSAGES_KEY}_{feedId}";
}

