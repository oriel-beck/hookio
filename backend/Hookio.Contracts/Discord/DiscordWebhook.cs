using System.Text.Json.Serialization;

namespace Hookio.Contracts.Discord
{
    public class DiscordWebhook
    {
        [JsonPropertyName("application_id")]
        public string? ApplicationId { get; set; }
        
        public string? Avatar {  get; set; }

        [JsonPropertyName("channel_id")]
        public required string ChannelId { get; set; }

        [JsonPropertyName("guild_id")]
        public required string GuildId { get; set; }

        public required string Id { get; set; }
        
        public required string Token { get; set; }

        public required string Url { get; set; }
    }
}
