using System.Text.Json.Serialization;

namespace Hookio.Discord.Contracts
{
    public class DiscordPartialGuild
    {
        [JsonPropertyName("id")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public ulong Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("icon")]
        public string? Icon { get; set; }

        [JsonPropertyName("owner")]
        public bool Owner { get; set; }

        [JsonPropertyName("permissions")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public ulong Permissions { get; set; }

        public string? IconUrl
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Icon)) return null;
                return $"https://cdn.discordapp.com/icons/{Id}/{Icon}.png";
            }
        }
    }
}
