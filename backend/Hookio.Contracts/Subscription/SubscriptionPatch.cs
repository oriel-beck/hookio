using Hookio.Contracts.Message;

namespace Hookio.Contracts.Subscription
{
    public class SubscriptionPatch
    {
        public string? WebhookUrl {  get; set; }

        public List<MessageRequest>? Messages { get; set; }

        // url for yt, probably username for twitch
        public string? Source { get; set; }

        public string? WebhookUsername { get; set; }

        public string? WebhookAvatar {  get; set; }

        public bool ClearWebhookAvatar { get; set; }

        public bool ClearWebhookUsername { get; set; }
    }
}
