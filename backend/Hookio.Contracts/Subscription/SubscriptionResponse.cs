using Hookio.Contracts.Message;
using Hookio.Shared.Enums;

namespace Hookio.Contracts.Subscription
{
    public class SubscriptionResponse
    {
        public int Id { get; set; }

        public required string GuildId { get; set; }

        public SubscriptionType SubscriptionType { get; set; }
        
        public string? Source { get; set; }

        public List<MessageResponse> Messages { get; set; } = [];
    }
}
