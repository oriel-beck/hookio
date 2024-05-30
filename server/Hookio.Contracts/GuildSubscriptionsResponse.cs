namespace Hookio.Contracts
{
    public class GuildSubscriptionsResponse
    {
        public int Count { get; set; }
        public List<SubscriptionResponse> Subscriptions { get; set; }
    }
}
