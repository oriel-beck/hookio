using Hookio.Enunms;

namespace Hookio.Contracts
{
    public class AnnouncementResponse
    {
        public int Id { get; set; }
        public AnnouncementType AnnouncementType { get; set; }
        // The name/identifier of the channel (yt, twitch, kick, etc) the announcement is linked to
        public string Origin { get; set; }
        // data sent to discord
        public string Message { get; set; }
        // embed data somehow? json?
    }
}
