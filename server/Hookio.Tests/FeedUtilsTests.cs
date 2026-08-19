using System.Xml.Linq;
using Hookio.Feeds;

namespace Hookio.Tests;

public class FeedUtilsTests
{
    [Fact]
    public void Parse_uses_latest_atom_entry_not_feed_id()
    {
        var xml = XDocument.Parse("""
            <feed xmlns="http://www.w3.org/2005/Atom" xmlns:yt="http://www.youtube.com/xml/schemas/2015">
              <id>yt:channel:CHANNEL</id>
              <title>Channel</title>
              <entry>
                <id>yt:video:NEWEST</id>
                <title>Newest</title>
                <published>2026-08-19T10:00:00Z</published>
                <updated>2026-08-19T11:00:00Z</updated>
              </entry>
              <entry>
                <id>yt:video:OLDER</id>
                <title>Older</title>
                <published>2026-08-01T10:00:00Z</published>
                <updated>2026-08-01T10:00:00Z</updated>
              </entry>
            </feed>
            """);

        var (_, details) = FeedUtils.Parse(xml);
        Assert.Equal("yt:video:NEWEST", details.Id);
        Assert.Equal(new DateTime(2026, 8, 19, 10, 0, 0, DateTimeKind.Utc), details.Published);
        Assert.Equal(new DateTime(2026, 8, 19, 11, 0, 0, DateTimeKind.Utc), details.Updated);
    }

    [Fact]
    public void Same_id_different_updated_is_an_edit()
    {
        var first = FeedUtils.Parse(XDocument.Parse("""
            <feed xmlns="http://www.w3.org/2005/Atom">
              <entry>
                <id>yt:video:ABC</id>
                <published>2026-08-19T10:00:00Z</published>
                <updated>2026-08-19T10:00:00Z</updated>
              </entry>
            </feed>
            """)).Item2;

        var second = FeedUtils.Parse(XDocument.Parse("""
            <feed xmlns="http://www.w3.org/2005/Atom">
              <entry>
                <id>yt:video:ABC</id>
                <published>2026-08-19T10:00:00Z</published>
                <updated>2026-08-19T12:00:00Z</updated>
              </entry>
            </feed>
            """)).Item2;

        Assert.Equal(first.Id, second.Id);
        Assert.NotEqual(first.Updated, second.Updated);
    }
}
