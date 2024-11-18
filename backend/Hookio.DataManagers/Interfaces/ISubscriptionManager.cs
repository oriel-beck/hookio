using Hookio.Contracts.Subscription;

namespace Hookio.DataManagers.Interfaces
{
    public interface ISubscriptionManager
    {
        public Task<SubscriptionResponse?> Create(string guildId, SubscriptionRequest request, CancellationToken cancellationToken);
        public Task<IEnumerable<SubscriptionResponse?>> Get(string guildId, SubscriptionFilter request, CancellationToken cancellationToken);
        public Task<SubscriptionResponse?> Get(string guildId, int subscriptionId, CancellationToken cancellationToken);
        public Task<SubscriptionResponse?> Patch(string guildId, int subscriptionId, SubscriptionPatch patch, CancellationToken cancellationToken);
    }
}
