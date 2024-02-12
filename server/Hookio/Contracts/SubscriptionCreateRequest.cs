using Hookio.Enunms;

namespace Hookio.Contracts
{
    public class SubscriptionCreateRequest
    {
        public SubscriptionType AnnouncementType { get; set; }
        public string WebhookUrl { get; set; }
        // The name/identifier of the channel (yt, twitch, kick, etc) the announcement is linked to
        public string ChannelId { get; set; }
        // data sent to discord
    }
}
