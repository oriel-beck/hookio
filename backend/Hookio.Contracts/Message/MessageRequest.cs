using Hookio.Contracts.Embed;

namespace Hookio.Contracts.Message
{
    public class MessageRequest
    {
        public int? Id { get; set; } = null;

        public string? Content { get; set; } = string.Empty;

        public IEnumerable<EmbedRequest> Embeds { get; set; } = [];

        public bool IsValid
        {
            get
            {
                return string.IsNullOrEmpty(Content) || (Embeds.Any() && Embeds.All(e => e.IsValid));
            }
        }
    }
}
