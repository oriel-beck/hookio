namespace Hookio.Twitch.Interfaces;

public record TwitchEventSubRegistration(string Login, string BroadcasterId, IReadOnlyList<string> SubscriptionIds);

public interface ITwitchEventSubService
{
    Task<TwitchEventSubRegistration> RegisterAsync(string channelUrl, CancellationToken cancellationToken = default);
    Task UnregisterAsync(string? eventSubIdsCsv, CancellationToken cancellationToken = default);
}
