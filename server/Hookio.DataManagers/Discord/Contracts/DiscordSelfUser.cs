using System.Text.Json.Serialization;

namespace Hookio.Discord.Contracts
{
    public class DiscordSelfUser
    {
        [JsonPropertyName("id")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public ulong Id { get; set; }

        [JsonPropertyName("discriminator")]
        public string Discriminator { get; set; } = "0";

        [JsonPropertyName("username")]
        public string? Username { get; set; }

        [JsonPropertyName("avatar")]
        public string? Avatar { get; set; }

        [JsonPropertyName("global_name")]
        public string? GlobalName { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        public string GetAvatarUrl()
        {
            if (string.IsNullOrEmpty(Avatar))
            {
                if (Discriminator == "0") return $"https://cdn.discordapp.com/embed/avatars/{(Id >> 22) % 6}.png";
                int discriminator = int.Parse(Discriminator);
                return $"https://cdn.discordapp.com/embed/avatars/{discriminator % 5}.png";
            }

            if (Avatar.StartsWith("a_")) return $"https://cdn.discordapp.com/avatars/{Id}/{Avatar}.gif";
            return $"https://cdn.discordapp.com/avatars/{Id}/{Avatar}.png";
        }
    }
}
