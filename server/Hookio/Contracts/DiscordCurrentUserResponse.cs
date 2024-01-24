using System.Text.Json.Serialization;

namespace Hookio.Contracts
{
    public class DiscordCurrentUserResponse
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Avatar { get; set; }
        public string Discriminator { get; set; } = "0";
        [JsonPropertyName("global_name")]
        public string GlobalName { get; set; }
        public string Email { get; set; }
    }
}
