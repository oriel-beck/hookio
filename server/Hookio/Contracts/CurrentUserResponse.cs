namespace Hookio.Contracts
{
    public class CurrentUserResponse
    {
        public ulong Id { get; set; }
        public string Username { get; set; }
        public string Discriminator { get; set; } = "0";
        public int Premium { get; set; } = 0;
        public string Avatar { get; set; }
        public List<GuildResponse> Guilds { get; set; }
    }
}
