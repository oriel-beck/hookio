using Hookio.Database.Entities;
using Hookio.Database.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Hookio.Database;

public class FeedRepository(IDbContextFactory<HookioContext> contextFactory) : IFeedRepository
{
    public async Task<List<Feed>> GetAllFeeds(CancellationToken cancellationToken, bool includeDisabled = false)
    {
        using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.Feeds
            .Where(x => includeDisabled || x.Disabled == false)
            .Include(f => f.Subscriptions.Where(s => !s.Disabled))
                .ThenInclude(s => s.Events)
                    .ThenInclude(e => e.Message)
                        .ThenInclude(m => m.Embeds)
                            .ThenInclude(e => e.Fields)
            .ToListAsync(cancellationToken);
    }

    public async Task<Feed> CreateFeed(string rssUrl)
    {
        using var ctx = await contextFactory.CreateDbContextAsync();
        var existing = await ctx.Feeds.FirstOrDefaultAsync(f => f.Url == rssUrl);
        if (existing is not null)
        {
            if (existing.Disabled) existing.Disabled = false;
            await ctx.SaveChangesAsync();
            return existing;
        }

        var feed = new Feed { Url = rssUrl };
        ctx.Feeds.Add(feed);
        await ctx.SaveChangesAsync();
        return feed;
    }

    public async Task<Feed?> UpdateFeed(int feedId, Feed feed)
    {
        var ctx = await contextFactory.CreateDbContextAsync();
        var originalFeed = await ctx.Feeds.FirstOrDefaultAsync(f => f.Id == feedId);
        if (originalFeed == null) return null;
        originalFeed.LastPublishedAt = feed.LastPublishedAt;
        originalFeed.LastId = feed.LastId;
        originalFeed.Disabled = feed.Disabled;
        await ctx.SaveChangesAsync();
        return feed;
    }

    internal static async Task<Feed> GetOrCreateFeedAsync(HookioContext ctx, string rssUrl)
    {
        var existing = ctx.Feeds.Local.FirstOrDefault(f => f.Url == rssUrl)
            ?? await ctx.Feeds.FirstOrDefaultAsync(f => f.Url == rssUrl);
        if (existing is not null)
        {
            if (existing.Disabled) existing.Disabled = false;
            return existing;
        }

        var feed = new Feed { Url = rssUrl };
        ctx.Feeds.Add(feed);
        return feed;
    }
}
