using System.Net;
using System.Text;
using Hookio.Twitch;

namespace Hookio.Tests;

public class EventSubCallbackTests : IClassFixture<HookioWebApplicationFactory>
{
    private readonly HookioWebApplicationFactory _factory;
    public EventSubCallbackTests(HookioWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task Bad_hmac_returns_403()
    {
        var client = _factory.CreateClient();
        using var request = SignedRequest("{}", "sha256=deadbeef");
        var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Challenge_returns_plain_text()
    {
        var client = _factory.CreateClient();
        var body = """{"challenge":"test-challenge-value","subscription":{"status":"webhook_callback_verification_pending"}}""";
        var timestamp = DateTimeOffset.UtcNow.ToString("o");
        var messageId = "challenge-1";
        var hex = TwitchEventSubSignature.ComputeSha256Hex("test-eventsub-secret", messageId + timestamp + body);
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/twitch/eventsub")
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };
        request.Headers.TryAddWithoutValidation(TwitchEventSubSignature.MessageIdHeader, messageId);
        request.Headers.TryAddWithoutValidation(TwitchEventSubSignature.TimestampHeader, timestamp);
        request.Headers.TryAddWithoutValidation(TwitchEventSubSignature.SignatureHeader, "sha256=" + hex);
        request.Headers.TryAddWithoutValidation(TwitchEventSubSignature.MessageTypeHeader, "webhook_callback_verification");

        var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("test-challenge-value", await response.Content.ReadAsStringAsync());
    }

    private static HttpRequestMessage SignedRequest(string body, string signature)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/twitch/eventsub")
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };
        request.Headers.TryAddWithoutValidation(TwitchEventSubSignature.MessageIdHeader, "x");
        request.Headers.TryAddWithoutValidation(TwitchEventSubSignature.TimestampHeader, DateTimeOffset.UtcNow.ToString("o"));
        request.Headers.TryAddWithoutValidation(TwitchEventSubSignature.SignatureHeader, signature);
        request.Headers.TryAddWithoutValidation(TwitchEventSubSignature.MessageTypeHeader, "notification");
        return request;
    }
}
