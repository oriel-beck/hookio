using Hookio.Enunms;

namespace Hookio.Contracts
{
    public class SubscriptionResponse
    {
        public int Id { get; set; }
        public SubscriptionType AnnouncementType { get; set; }
        // The name/identifier of the channel (yt, twitch, kick, etc) the announcement is linked to
        public string ChannelId { get; set; }
        // data sent to discord
        public MessageResponse? Message { get; set; }
    }
}
