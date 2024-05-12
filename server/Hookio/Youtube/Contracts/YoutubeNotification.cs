using System.Xml.Serialization;

namespace Hookio.Utils.Contracts
{
    [XmlRoot(ElementName = "entry")]
    public class YoutubeNotification
    {
        [XmlElement(ElementName = "id")]
        public string Id { get; set; }

        [XmlElement(ElementName = "videoId", Namespace = "http://www.youtube.com/xml/schemas/2015")]
        public string VideoId { get; set; }

        [XmlElement(ElementName = "channelId")]
        public string ChannelId { get; set; }

        [XmlElement(ElementName = "title")]
        public string Title { get; set; }

        [XmlElement(ElementName = "link")]
        public Link Link { get; set; }

        [XmlElement(ElementName = "author")]
        public Author Author { get; set; }

        [XmlElement(ElementName = "published")]
        public string Published { get; set; }

        [XmlElement(ElementName = "updated")]
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
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }

        [XmlElement(ElementName = "uri")]
        public string Uri { get; set; }
    }

    public class Link
    {
        [XmlAttribute(AttributeName = "href")]
        public string Href { get; set; }
    }
}
