using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hookio.Database.Entities
{
    public class Embed
    {
        [Key]
        public int Id { get; set; }
        public string? Description { get; set; }
        public string? Url { get; set; }
        public string? Title { get; set; }
        public string? Color { get; set; }
        public string? Image { get; set; }
        public string? Author { get; set; }
        public string? AuthorIcon { get; set; }
        public string? Footer { get; set; }
        public string? FooterIcon { get; set; }
        public string? Thumbnail { get; set; }

        public ImmutableArray<EmbedField> Fields { get; set; } = [];
        public int MessageId { get; set; }
        [ForeignKey("MessageId")]
        public Message? Message { get; set; }

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
                int valueOrDefault3 = Fields.Sum((EmbedField f) => f.Name?.Length + f.Value?.ToString().Length).GetValueOrDefault();
                return num + valueOrDefault + num2 + valueOrDefault2 + valueOrDefault3;
            }
        }
    }
}
