using System.ComponentModel.DataAnnotations;

namespace Hookio.Contracts
{
    public class MessageRequest
    {
        public int? Id { get; set; }
        [MaxLength(2000)]
        public string Content { get; set; } = string.Empty;
        public EmbedRequest[] Embeds { get; set; } = [];
        public string? Username { get; set; }
        public string? Avatar { get; set; }
        // TODO: pass avatar file, convert to base64 and save in db for now.
        //public byte[]? Avatar { get; set; }
    }
}
