using Hookio.Enunms;

namespace Hookio.Contracts
{
    public class SubscriptionRequest
    {
        public SubscriptionType SubscriptionType { get; set; }
        public string WebhookUrl { get; set; }
        // The url of the channel (yt, twitch, custom) the announcement is linked to
        public string Url { get; set; }
        // data sent to discord
        public Dictionary<EventType, EventRequest> Events { get; set; }
    }
}
