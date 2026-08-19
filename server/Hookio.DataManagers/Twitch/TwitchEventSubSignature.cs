using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Hookio.Twitch;

/// <summary>
/// Twitch EventSub HMAC-SHA256: https://dev.twitch.tv/docs/eventsub/handling-webhook-events
/// </summary>
public static class TwitchEventSubSignature
{
    public const string MessageIdHeader = "Twitch-Eventsub-Message-Id";
    public const string TimestampHeader = "Twitch-Eventsub-Message-Timestamp";
    public const string SignatureHeader = "Twitch-Eventsub-Message-Signature";
    public const string MessageTypeHeader = "Twitch-Eventsub-Message-Type";

    public static bool IsValid(string secret, string messageId, string timestamp, string body, string? providedSignature, TimeSpan? maxAge = null)
    {
        if (string.IsNullOrEmpty(secret) || string.IsNullOrEmpty(messageId) || string.IsNullOrEmpty(timestamp) || providedSignature is null)
            return false;

        maxAge ??= TimeSpan.FromMinutes(10);
        if (!DateTimeOffset.TryParse(timestamp, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var sentAt))
            return false;
        if (DateTimeOffset.UtcNow - sentAt.ToUniversalTime() > maxAge)
            return false;

        var expected = ComputeSha256Hex(secret, messageId + timestamp + body);
        var provided = providedSignature.StartsWith("sha256=", StringComparison.OrdinalIgnoreCase)
            ? providedSignature["sha256=".Length..]
            : providedSignature;

        var expectedBytes = Convert.FromHexString(expected);
        byte[] providedBytes;
        try
        {
            providedBytes = Convert.FromHexString(provided);
        }
        catch (FormatException)
        {
            return false;
        }

        return expectedBytes.Length == providedBytes.Length
            && CryptographicOperations.FixedTimeEquals(expectedBytes, providedBytes);
    }

    public static string ComputeSha256Hex(string secret, string payload)
    {
        var key = Encoding.UTF8.GetBytes(secret);
        var data = Encoding.UTF8.GetBytes(payload);
        var hash = HMACSHA256.HashData(key, data);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
