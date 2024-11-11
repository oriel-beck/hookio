using Hookio.Contracts.Embed;

namespace Hookio.Contracts.Message
{
    public class MessageRequest
    {
        public required int SubscriptionId { get; set; }

        public string? Content { get; set; }

        public IEnumerable<EmbedRequest> Embeds { get; set; } = [];

        public bool IsValid
        {
            get
            {
                return Content != null || (Embeds.Any() && Embeds.All(e => e.IsValid));
            }
        }
    }
}
