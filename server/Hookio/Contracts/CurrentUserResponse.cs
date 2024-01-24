using System.Text.Json.Serialization;

namespace Hookio.Contracts
{
    public class CurrentUserResponse
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Discriminator { get; set; } = "0";
        public int Premium { get; set; } = 0;
        public string Avatar { get; set; }
        public List<GuildResponse> Guilds { get; set; }
    }
}
