using Hookio.Enums;

namespace Hookio.Contracts
{
    public class SubscriptionResponse
    {
        public required int Id { get; set; }
        public required ulong GuildId { get; set; }
        public required SubscriptionType SubscriptionType { get; set; }
        public required Dictionary<EventType, EventResponse>? Events { get; set; }
        public required ulong ChannelId { get; set; }
        /// <summary>Channel URL for the editor (YouTube reconstructed from the RSS feed URL).</summary>
        public string? Url { get; set; }
        public bool Disabled { get; set; }
    }
}
