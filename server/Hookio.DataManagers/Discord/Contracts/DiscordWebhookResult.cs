namespace Hookio.Discord.Contracts;

public readonly record struct DiscordWebhookResult(int StatusCode, DiscordPartialMessage? Message)
{
    public bool Success => StatusCode is >= 200 and < 300 && Message is not null;
    public bool UnauthorizedOrMissing => StatusCode is 401 or 404;
}
