namespace Hookio.Contracts
{
    public class FeedResponse
    {
        public required string Url { get; set; }
        public required List<TemplateStringResponse> TemplateStrings { get; set; }
        public required List<SubscriptionResponse> Subscriptions { get; set; }
    }
}
