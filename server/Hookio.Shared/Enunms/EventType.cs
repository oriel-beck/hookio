namespace Hookio.Enunms
{
    public enum EventType
    {
        // general events (yt and everything else)
        NewFeed = 1,
        UpdatedFeed,
        // special twitch events
        TwitchStreamStarted,
        TwitchStreamUpdated,
        TwitchStreamEnded
    }
}
