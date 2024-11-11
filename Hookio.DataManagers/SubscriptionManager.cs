using Hookio.Contracts.Subscription;
using Hookio.Data;
using Hookio.DataManagers.Interfaces;

namespace Hookio.DataManagers
{
    public class SubscriptionManager(HookioContextFactory contextFactory) : ISubscriptionManager
    {
        private readonly HookioContextFactory _contextFactory = contextFactory;

        public Task<SubscriptionResponse?> Create(ulong guildId, SubscriptionRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<SubscriptionResponse>> Get(ulong guildId, SubscriptionFilter request)
        {
            throw new NotImplementedException();
        }

        public Task<SubscriptionResponse?> Get(ulong guildId, int subscriptionId)
        {
            throw new NotImplementedException();
        }

        public Task<SubscriptionResponse> Update(ulong guildId, int subscriptionId, SubscriptionRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
