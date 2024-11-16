namespace Hookio.Contracts.Discord
{
    public class DiscordGuild
    {
        public required ulong Id { get; set; }
        public required string Name { get; set; }
        public string? IconUrl { get; set; }
    }
}
