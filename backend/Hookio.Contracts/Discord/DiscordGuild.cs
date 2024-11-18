namespace Hookio.Contracts.Discord
{
    public class DiscordGuild
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public string? IconUrl { get; set; }
    }
}
