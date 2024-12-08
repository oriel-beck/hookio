using System.Xml.Serialization;

namespace Hookio.Contracts.YouTube
{
    [XmlRoot(ElementName = "feed", Namespace = "http://www.w3.org/2005/Atom")]
    public class YouTubeFeed
    {
        [XmlElement(ElementName = "link", Namespace = "http://www.w3.org/2005/Atom")]
        public required string Links { get; set; }

        [XmlElement(ElementName = "title", Namespace = "http://www.w3.org/2005/Atom")]
        public required string Title { get; set; }

        [XmlElement(ElementName = "updated", Namespace = "http://www.w3.org/2005/Atom")]
        public DateTime Updated { get; set; }

        [XmlElement(ElementName = "entry", Namespace = "http://www.w3.org/2005/Atom")]
        public required YouTubeEntry Entry { get; set; }
    }

    public class Link
    {
        [XmlAttribute(AttributeName = "rel")]
        public required string Rel { get; set; }

        [XmlAttribute(AttributeName = "href")]
        public required string Href { get; set; }
    }

    public class YouTubeEntry
    {
        [XmlElement(ElementName = "id", Namespace = "http://www.w3.org/2005/Atom")]
        public required string Id { get; set; }

        [XmlElement(ElementName = "videoId", Namespace = "http://www.youtube.com/xml/schemas/2015")]
        public required string VideoId { get; set; }

        [XmlElement(ElementName = "channelId", Namespace = "http://www.youtube.com/xml/schemas/2015")]
        public required string ChannelId { get; set; }

        [XmlElement(ElementName = "title", Namespace = "http://www.w3.org/2005/Atom")]
        public required string Title { get; set; }

        [XmlElement(ElementName = "link", Namespace = "http://www.w3.org/2005/Atom")]
        public required Link Link { get; set; }

        [XmlElement(ElementName = "author", Namespace = "http://www.w3.org/2005/Atom")]
        public required Author Author { get; set; }

        [XmlElement(ElementName = "published", Namespace = "http://www.w3.org/2005/Atom")]
        public DateTime Published { get; set; }

        [XmlElement(ElementName = "updated", Namespace = "http://www.w3.org/2005/Atom")]
        public DateTime Updated { get; set; }
    }

    public class Author
    {
        [XmlElement(ElementName = "name", Namespace = "http://www.w3.org/2005/Atom")]
        public required string Name { get; set; }

        [XmlElement(ElementName = "uri", Namespace = "http://www.w3.org/2005/Atom")]
        public required string Uri { get; set; }
    }

}
