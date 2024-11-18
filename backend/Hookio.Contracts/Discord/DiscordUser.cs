namespace Hookio.Contracts.Discord
{
    public class DiscordUser
    {
        public required string Id { get; set; }
        public string? GlobalName { get; set; }
        public required string Username { get; set; }
        public required string AvatarUrl { get; set; }
    }
}
