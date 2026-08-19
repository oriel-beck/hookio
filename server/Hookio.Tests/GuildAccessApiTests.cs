using Hookio.Shared;

namespace Hookio.Tests;

public class GuildAccessApiTests : IClassFixture<HookioWebApplicationFactory>
{
    private readonly HookioWebApplicationFactory _factory;

    public GuildAccessApiTests(HookioWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task Subscriptions_without_cookie_are_unauthorized()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/subscriptions/199737254929760256");
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Subscriptions_for_other_guild_are_unauthorized()
    {
        var client = Authed("199737254929760256");
        var response = await client.GetAsync("/api/subscriptions/199737254929760257");
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Subscriptions_for_claimed_guild_are_ok()
    {
        var client = Authed("199737254929760256");
        var response = await client.GetAsync("/api/subscriptions/199737254929760256");
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Authenticate_with_mocked_discord_sets_cookie()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsync("/api/users/authenticate?code=placeholder-code", null);
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.TryGetValues("Set-Cookie", out var cookies));
        Assert.Contains(AuthConstants.CookieName, string.Join(";", cookies));
    }

    private HttpClient Authed(string guildId)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("Cookie", $"{AuthConstants.CookieName}={TestJwt.Create("1", guildId)}");
        return client;
    }
}
