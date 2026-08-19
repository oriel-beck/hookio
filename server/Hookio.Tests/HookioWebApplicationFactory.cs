using Hookio.Database;
using Hookio.Twitch.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using NSubstitute;
using StackExchange.Redis;

namespace Hookio.Tests;

public sealed class HookioWebApplicationFactory : WebApplicationFactory<Program>
{
    public DiscordMockHandler Discord { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        Environment.SetEnvironmentVariable("JWT_SECRET", TestJwt.Secret);
        Environment.SetEnvironmentVariable("DISCORD_CLIENT_ID", "placeholder-discord-client-id");
        Environment.SetEnvironmentVariable("DISCORD_CLIENT_SECRET", "placeholder-discord-client-secret");
        Environment.SetEnvironmentVariable("DISCORD_REDIRECT_URI", "http://localhost/oauth");
        Environment.SetEnvironmentVariable("TWITCH_EVENTSUB_SECRET", "test-eventsub-secret");
        Environment.SetEnvironmentVariable("TWITCH_EVENTSUB_CALLBACK_URL", "https://example.invalid/api/twitch/eventsub");
        Environment.SetEnvironmentVariable("TWITCH_CLIENT_ID", "placeholder-twitch-client-id");
        Environment.SetEnvironmentVariable("TWITCH_CLIENT_SECRET", "placeholder-twitch-client-secret");

        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IHostedService>();
            services.AddDbContextFactory<HookioContext>(o => o.UseInMemoryDatabase("hookio-tests"));

            var db = Substitute.For<IDatabase>();
            db.PingAsync(Arg.Any<CommandFlags>()).Returns(TimeSpan.FromMilliseconds(1));
            db.StringGetAsync(Arg.Any<RedisKey>(), Arg.Any<CommandFlags>()).Returns(RedisValue.Null);
            db.StringSetAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue>(), Arg.Any<TimeSpan?>(), Arg.Any<When>(), Arg.Any<CommandFlags>()).Returns(true);
            db.KeyDeleteAsync(Arg.Any<RedisKey>(), Arg.Any<CommandFlags>()).Returns(true);
            var mux = Substitute.For<IConnectionMultiplexer>();
            mux.GetDatabase(Arg.Any<int>(), Arg.Any<object>()).Returns(db);
            services.AddSingleton(mux);

            services.RemoveAll<IHttpClientFactory>();
            services.AddSingleton<IHttpClientFactory>(new MockHttpClientFactory(Discord));

            var twitch = Substitute.For<ITwitchEventSubService>();
            twitch.RegisterAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(new TwitchEventSubRegistration("login", "123", ["esub-1"]));
            services.RemoveAll<ITwitchEventSubService>();
            services.AddSingleton(twitch);

            services.AddHostedService(sp => sp.GetRequiredService<Hookio.Utils.TaskQueue>());
        });
    }
}

internal sealed class MockHttpClientFactory(HttpMessageHandler handler) : IHttpClientFactory
{
    public HttpClient CreateClient(string name) => new(handler, disposeHandler: false)
    {
        BaseAddress = new Uri("https://discord.com")
    };
}

public sealed class DiscordMockHandler : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var path = request.RequestUri?.AbsolutePath ?? "";
        HttpResponseMessage response;
        if (path.Contains("/api/webhooks/", StringComparison.OrdinalIgnoreCase) && request.Method == HttpMethod.Get)
        {
            response = Json("""{"id":"1","channel_id":"10","guild_id":"199737254929760256"}""");
        }
        else if (path.Contains("/oauth2/token", StringComparison.OrdinalIgnoreCase))
        {
            response = Json("""{"access_token":"at","token_type":"Bearer","expires_in":604800,"refresh_token":"rt","scope":"identify guilds email"}""");
        }
        else if (path.EndsWith("/users/@me/guilds", StringComparison.OrdinalIgnoreCase))
        {
            response = Json("""[{"id":"199737254929760256","name":"Test","icon":null,"owner":true,"permissions":"32"}]""");
        }
        else if (path.EndsWith("/users/@me", StringComparison.OrdinalIgnoreCase))
        {
            response = Json("""{"id":"1","username":"tester","discriminator":"0","global_name":"Tester","avatar":null,"email":"t@example.com"}""");
        }
        else
        {
            response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json")
            };
        }
        return Task.FromResult(response);
    }

    private static HttpResponseMessage Json(string json) => new(System.Net.HttpStatusCode.OK)
    {
        Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
    };
}
