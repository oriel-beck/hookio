﻿using Hookio.Enunms;

namespace Hookio.Contracts
{
    public class SubscriptionResponse
    {
        public int Id { get; set; }
        public ulong GuildId { get; set; }
        public SubscriptionType SubscriptionType { get; set; }
        // The name/identifier of the channel (yt, twitch, kick, etc) the announcement is linked to
        public string Url { get; set; }
        // data sent to discord
        public Dictionary<EventType, EventResponse>? Events { get; set; }
    }
}
