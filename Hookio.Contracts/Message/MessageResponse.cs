using Hookio.Contracts.Embed;

namespace Hookio.Contracts.Message
{
    public class MessageResponse
    {
        public required int Id { get; set; }

        public string? Content { get; set; } = string.Empty;

        public IEnumerable<EmbedRequest> Embeds { get; set; } = [];
    }
}
