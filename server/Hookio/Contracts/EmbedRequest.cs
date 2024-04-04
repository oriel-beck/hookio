using System.ComponentModel.DataAnnotations;

namespace Hookio.Contracts
{
    public class EmbedRequest
    {
        public int? Id { get; set; }
        [MaxLength(4096)]
        public string? Description { get; set; }
        public string? TitleUrl { get; set; }
        [MaxLength(256)]
        public string? Title { get; set; }
        public string? Color { get; set; }
        public string? Image { get; set; }
        [MaxLength(256)]
        public string? Author { get; set; }
        public string? AuthorUrl { get; set; }
        public string? AuthorIcon { get; set; }
        [MaxLength(2048)]
        public string? Footer { get; set; }
        public string? FooterIcon { get; set; }
        public string? Thumbnail { get; set; }
        public bool AddTimestamp { get; set; }

        public List<EmbedFieldRequest> Fields { get; set; } = [];
        
        //
        // Summary:
        //     Gets the total length of all embed properties.
        public int Length
        {
            get
            {
                int num = Title?.Length ?? 0;
                int valueOrDefault = (Author?.Length).GetValueOrDefault();
                int num2 = Description?.Length ?? 0;
                int valueOrDefault2 = (Footer?.Length).GetValueOrDefault();
                int valueOrDefault3 = Fields.Sum((EmbedFieldRequest f) => f.Name?.Length + f.Value?.ToString().Length).GetValueOrDefault();
                return num + valueOrDefault + num2 + valueOrDefault2 + valueOrDefault3;
            }
        }
    }
}
