using Hookio.Contracts.Subscription;

namespace Hookio.DataManagers.Interfaces
{
    public interface ISubscriptionManager
    {
        public Task<SubscriptionResponse?> Create(ulong guildId, SubscriptionRequest request);
        public Task<IEnumerable<SubscriptionResponse>> Get(ulong guildId, SubscriptionFilter request);
        public Task<SubscriptionResponse?> Get(ulong guildId, int subscriptionId);
        public Task<SubscriptionResponse> Update(ulong guildId, int subscriptionId, SubscriptionRequest request);
    }
}
