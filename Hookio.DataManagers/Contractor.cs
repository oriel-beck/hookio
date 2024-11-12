using Hookio.Contracts.Message;
using Hookio.Contracts.Subscription;
using Hookio.Data.Entities;

namespace Hookio.DataManagers
{
    public static class Contractor
    {
        public static SubscriptionResponse? ToContract(Subscription? subscription) =>
            subscription == null ?
            null :
            new()
            {
                Id = subscription.Id,
                GuildId = subscription.GuildId,
                Source = "", // tmp
                SubscriptionType = subscription.SubscriptionType,
                Messages = subscription.Messages?.Select(ToContract) ?? []
            };

        public static MessageResponse ToContract(Message message) =>
            new()
            {
                Content = message.Content,
                Id = message.Id,
                Embeds = message.Embeds ?? []
            };
    }
}
