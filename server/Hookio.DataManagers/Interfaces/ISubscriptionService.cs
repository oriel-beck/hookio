using Hookio.Contracts;
using Hookio.Enums;

namespace Hookio.Database.Interfaces;

public interface ISubscriptionService
{
    Task<SubscriptionResponse?> GetSubscription(ulong guildId, int id);
    Task<SubscriptionResponse?> CreateSubscription(ulong guildId, SubscriptionRequest request);
    Task<SubscriptionResponse?> UpdateSubscription(ulong guildId, int id, SubscriptionRequest request);
    Task<GuildSubscriptionsResponse> GetSubscriptions(ulong guildId, SubscriptionType? provider, bool withCounts = false);
    Task<bool> DeleteSubscription(ulong guildId, int id);
    Task DisableSubscription(int subscriptionId, string reason);
    Task<IReadOnlyList<Database.Entities.Subscription>> GetEnabledTwitchSubscriptions(string broadcasterId, CancellationToken cancellationToken = default);
}
