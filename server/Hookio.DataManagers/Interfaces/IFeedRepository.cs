using Hookio.Database.Entities;

namespace Hookio.Database.Interfaces;

public interface IFeedRepository
{
    Task<List<Feed>> GetAllFeeds(CancellationToken cancellationToken, bool includeDisabled = false);
    Task<Feed> CreateFeed(string rssUrl);
    Task<Feed?> UpdateFeed(int feedId, Feed feed);
}
