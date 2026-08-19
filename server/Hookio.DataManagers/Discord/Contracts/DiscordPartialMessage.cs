using System.Text.Json.Serialization;

namespace Hookio.Discord.Contracts
{
    public class DiscordPartialMessage
    {
        [JsonPropertyName("id")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public ulong Id { get; set; }
    }
}
