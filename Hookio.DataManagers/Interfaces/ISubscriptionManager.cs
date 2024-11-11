using Hookio.Contracts.Subscription;

namespace Hookio.DataManagers.Interfaces
{
    public interface ISubscriptionManager
    {
        public Task<SubscriptionResponse?> Create(ulong guildId, SubscriptionRequest request, CancellationToken cancellationToken);
        public Task<IEnumerable<SubscriptionResponse?>> Get(ulong guildId, SubscriptionFilter request, CancellationToken cancellationToken);
        public Task<SubscriptionResponse?> Get(ulong guildId, int subscriptionId, CancellationToken cancellationToken);
        public Task<SubscriptionResponse?> Patch(ulong guildId, int subscriptionId, SubscriptionPatch patch, CancellationToken cancellationToken);
    }
}
