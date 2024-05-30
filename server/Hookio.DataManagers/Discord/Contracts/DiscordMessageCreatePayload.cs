using Discord;

namespace Hookio.Discord.Contracts
{
    public class DiscordMessageCreatePayload
    {
        public string? Content { get; set; }
        public IEnumerable<Embed> Embeds { get; set; }
        public string? Avatar { get; set; }
        public string? Username { get; set; }
    }
}
