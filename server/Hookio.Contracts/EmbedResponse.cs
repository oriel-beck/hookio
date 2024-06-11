namespace Hookio.Contracts
{
    public class EmbedResponse
    {
        public required int Id { get; set; }
        public required int Index { get; set; }
        public string? Description { get; set; }
        public string? TitleUrl { get; set; }
        public string? Title { get; set; }
        public string? Color { get; set; }
        public string? Image { get; set; }
        public string? Author { get; set; }
        public string? AuthorUrl { get; set; }
        public string? AuthorIcon { get; set; }
        public string? Footer { get; set; }
        public string? FooterIcon { get; set; }
        public string? Thumbnail { get; set; }
        public required bool AddTimestamp { get; set; }
        public required List<EmbedFieldResponse> Fields { get; set; } = [];
    }
}
