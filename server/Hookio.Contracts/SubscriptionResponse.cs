using Hookio.Enunms;

namespace Hookio.Contracts
{
    public class SubscriptionResponse
    {
        public required int Id { get; set; }
        public required ulong GuildId { get; set; }
        public required SubscriptionType SubscriptionType { get; set; }
        // data sent to discord
        public required Dictionary<EventType, EventResponse>? Events { get; set; }
        public required ulong ChannelId { get; set; }
    }
}
