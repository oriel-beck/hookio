namespace Hookio.Youtube.Contracts
{
    public class YoutubeNotification
    {
        public string Id { get; set; }
        public string VideoId { get; set; }
        public string ChannelId { get; set; }
        public string Title { get; set; }
        public string Link { get; set; }
        public Author Author { get; set; }
        public string Published { get; set; }
        public string Updated { get; set; }
        public bool IsNewVideo
        {
            get
            {
                return Published == Updated && !string.IsNullOrEmpty(Published);
            }
        }
    }

    public class Author
    {
        public string Name { get; set; }
        public string Uri { get; set; }
    }
}
