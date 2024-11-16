using Hookio.Contracts.Discord;

namespace Hookio.Contracts.User
{
    public class CurrentUserResponse
    {
        public required DiscordUser User { get; set; }
        public required IEnumerable<DiscordGuild> Guilds { get; set; }
    }
}
