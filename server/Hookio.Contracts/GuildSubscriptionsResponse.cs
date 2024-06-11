namespace Hookio.Contracts
{
    public class GuildSubscriptionsResponse
    {
        public required int Count { get; set; }
        public required List<SubscriptionResponse> Subscriptions { get; set; }
    }
}
