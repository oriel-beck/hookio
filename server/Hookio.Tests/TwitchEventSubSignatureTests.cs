using Hookio.Twitch;

namespace Hookio.Tests;

public class TwitchEventSubSignatureTests
{
    private const string Secret = "test-eventsub-secret";

    [Fact]
    public void Valid_hmac_matches()
    {
        var messageId = "msg-1";
        var timestamp = DateTimeOffset.UtcNow.ToString("o");
        var body = """{"challenge":"abc"}""";
        var hex = TwitchEventSubSignature.ComputeSha256Hex(Secret, messageId + timestamp + body);
        Assert.True(TwitchEventSubSignature.IsValid(Secret, messageId, timestamp, body, "sha256=" + hex));
    }

    [Fact]
    public void Bad_hmac_is_rejected()
    {
        var timestamp = DateTimeOffset.UtcNow.ToString("o");
        Assert.False(TwitchEventSubSignature.IsValid(Secret, "id", timestamp, "{}", "sha256=deadbeef"));
    }

    [Fact]
    public void Stale_timestamp_is_rejected()
    {
        var timestamp = DateTimeOffset.UtcNow.AddMinutes(-11).ToString("o");
        var body = "{}";
        var hex = TwitchEventSubSignature.ComputeSha256Hex(Secret, "id" + timestamp + body);
        Assert.False(TwitchEventSubSignature.IsValid(Secret, "id", timestamp, body, "sha256=" + hex));
    }
}
