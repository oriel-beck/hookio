using Hookio.Shared.Enums;

namespace Hookio.Contracts.Subscription
{
    public class SubscriptionRequest
    {
        public required SubscriptionType SubscriptionType { get; set; }

        public required string WebhookUrl {  get; set; }

        // url for yt, probably username for twitch
        public string? Source { get; set; }
    }
}
