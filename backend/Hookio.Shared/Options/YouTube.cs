namespace Hookio.Shared.Options
{
    public class YouTube
    {
        public required string Key { get; set; }
        
        public required string HubCallback { get; set; }

        public required string HubSecret { get; set; }

        public string YouTubeFeedBaseUrl = "https://www.youtube.com/xml/feeds/videos.xml?channel_id=";
    }
}
