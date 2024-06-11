namespace Hookio.Contracts
{
    public class EmbedFieldResponse
    {
        public required int Index { get; set; }
        public required int Id { get; set; }
        public required string Name { get; set; }
        public required string Value { get; set; }
        public bool Inline { get; set; }
    }
}
