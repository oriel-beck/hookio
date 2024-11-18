using Hookio.Contracts.Embed;
using Hookio.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace Hookio.Contracts.Message
{
    public class MessageRequest
    {
        public int? Id { get; set; } = null;

        [MaxLength(2048)]
        public string? Content { get; set; } = string.Empty;

        public List<EmbedRequest> Embeds { get; set; } = [];

        public required MessageAction? Action { get; set; }

        public required MessageType Type { get; set; }

        public bool IsValid
        {
            get
            {
                // only valid if message action is deleteMessage (which ignores content and embeds) or message content is not empty or null and if there are any embeds they are all valid
                return Action == MessageAction.DeleteMessage || (!string.IsNullOrEmpty(Content) && ((Embeds.Any() && Embeds.All(e => e.IsValid)) || !Embeds.Any()));
            }
        }
    }
}
