namespace Hookio.Contracts
{
    public class MessageResponse
    {
        public int? Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public List<EmbedResponse> Embeds { get; set; }
        public string? Username { get; set; }
        public string? Avatar { get; set; }
        // TODO: pass avatar file, convert to base64 and save in db for now.
        //public byte[]? Avatar { get; set; }
    }
}
