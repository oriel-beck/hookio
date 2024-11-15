using Discord.Rest;

namespace Hookio.Contracts.User
{
    public class CurrentUserResponse
    {
        public required RestSelfUser User { get; set; }
        public required IEnumerable<RestUserGuild> Guilds { get; set; }
    }
}
