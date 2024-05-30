namespace Hookio.Contracts
{
    public class FeedResponse
    {
        public string Url { get; set; }
        public List<TemplateStringResponse> TemplateStrings { get; set; }
        public List<SubscriptionResponse> Subscriptions { get; set; }
    }
}
