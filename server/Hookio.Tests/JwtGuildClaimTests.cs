using System.Security.Claims;
using System.Text.Json;
using Hookio;
using Hookio.Shared;

namespace Hookio.Tests;

public class JwtGuildClaimTests
{
    [Fact]
    public void CanAccessGuild_allows_listed_guild_id()
    {
        var user = Principal(["199737254929760256", "2"]);
        Assert.True(Util.CanAccessGuild(user, 199737254929760256UL));
    }

    [Fact]
    public void CanAccessGuild_rejects_other_guild()
    {
        var user = Principal(["1"]);
        Assert.False(Util.CanAccessGuild(user, 2));
    }

    [Fact]
    public void CanAccessGuild_rejects_missing_claim()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity([new Claim(AuthConstants.IdClaim, "1")], "test"));
        Assert.False(Util.CanAccessGuild(user, 1));
    }

    private static ClaimsPrincipal Principal(string[] guilds)
    {
        var identity = new ClaimsIdentity(
        [
            new Claim(AuthConstants.IdClaim, "1"),
            new Claim(AuthConstants.GuildsClaim, JsonSerializer.Serialize(guilds))
        ], "test");
        return new ClaimsPrincipal(identity);
    }
}
