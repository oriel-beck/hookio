using Hookio.Feeds;
using NSubstitute;
using StackExchange.Redis;

namespace Hookio.Tests;

public class RedisScoreTests
{
    [Fact]
    public async Task InsertNewFeed_uses_unix_seconds_not_milliseconds()
    {
        var db = Substitute.For<IDatabase>();
        var mux = Substitute.For<IConnectionMultiplexer>();
        mux.GetDatabase(Arg.Any<int>(), Arg.Any<object>()).Returns(db);
        double? score = null;
        db.SortedSetAddAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue>(), Arg.Do<double>(s => score = s), Arg.Any<CommandFlags>())
            .Returns(true);
        db.SortedSetAddAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue>(), Arg.Do<double>(s => score = s), Arg.Any<SortedSetWhen>(), Arg.Any<CommandFlags>())
            .Returns(true);
        db.SortedSetAddAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue>(), Arg.Do<double>(s => score = s), Arg.Any<When>(), Arg.Any<CommandFlags>())
            .Returns(true);

        var cache = new FeedsCacheService(mux);
        await cache.InsertNewFeed(42);

        Assert.NotNull(score);
        var seconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Assert.InRange(score!.Value, seconds - 5, seconds + 5);
        Assert.True(score < 4_000_000_000d);
        Assert.True(score > 1_000_000_000d);
    }

    [Fact]
    public async Task Message_lookup_is_by_subscription_id()
    {
        var db = Substitute.For<IDatabase>();
        var mux = Substitute.For<IConnectionMultiplexer>();
        mux.GetDatabase(Arg.Any<int>(), Arg.Any<object>()).Returns(db);
        RedisValue field = default;
        db.HashSetAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue>(), Arg.Any<RedisValue>(), Arg.Any<When>(), Arg.Any<CommandFlags>())
            .Returns(call =>
            {
                field = call.ArgAt<RedisValue>(1);
                return true;
            });
        db.HashGetAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue>(), Arg.Any<CommandFlags>())
            .Returns(_ => (RedisValue)"99");

        var cache = new FeedsCacheService(mux);
        await cache.InsertNewMessage(7, 15, 99);
        var messageId = await cache.GetMessageId(7, 15);

        Assert.Equal("15", field.ToString());
        Assert.Equal(99UL, messageId);
    }
}
