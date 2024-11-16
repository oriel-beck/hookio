using Hookio.Contracts.Embed;
using Hookio.Shared.Enums;

namespace Hookio.Contracts.Message
{
    public class MessageResponse
    {
        public required int Id { get; set; }

        public string? Content { get; set; } = string.Empty;

        public IEnumerable<EmbedRequest> Embeds { get; set; } = [];

        public required MessageAction? Action { get; set; }

        public required MessageType Type { get; set; }
    }
}
