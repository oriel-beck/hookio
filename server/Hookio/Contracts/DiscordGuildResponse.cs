namespace Hookio.Contracts
{
    public class DiscordGuildResponse
    {
        public string Id { get; set; }
        public string? Icon { get; set; }
        public bool Owner { get; set; }
        public string Name { get; set; }
        public string Permissions { get; set; }
    }
}