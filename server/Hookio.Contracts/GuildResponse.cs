namespace Hookio.Contracts
{
    public class GuildResponse
    {
        public required string Name { get; set; }
        public string? Icon { get; set; }
        public required string Id { get; set; }
    }
}
