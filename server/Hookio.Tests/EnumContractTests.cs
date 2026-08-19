using Hookio.Enums;

namespace Hookio.Tests;

public class EnumContractTests
{
    [Fact]
    public void EventType_numbers_match_client_contract()
    {
        Assert.Equal(1, (int)EventType.NewFeed);
        Assert.Equal(2, (int)EventType.UpdatedFeed);
        Assert.Equal(3, (int)EventType.TwitchStreamStarted);
        Assert.Equal(4, (int)EventType.TwitchStreamUpdated);
        Assert.Equal(5, (int)EventType.TwitchStreamEnded);
    }

    [Fact]
    public void SubscriptionType_keeps_twitch()
    {
        Assert.Equal(1, (int)SubscriptionType.Youtube);
        Assert.Equal(2, (int)SubscriptionType.Twitch);
        Assert.Equal(3, (int)SubscriptionType.Custom);
    }
}
