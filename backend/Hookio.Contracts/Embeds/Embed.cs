using Hookio.Shared;
using System.ComponentModel.DataAnnotations;

// TODO: URL Validation attribute
namespace Hookio.Contracts.Embed
{
    public class EmbedRequest
    {
        [MaxLength(256)]
        public string? Title { get; set; }

        [MaxLength(4096)]
        public string? Description { get; set; }

        public string? Url { get; set; }

        public DateTime? Timestamp { get; set; }

        public uint? Color { get; set; }

        public Footer? Footer { get; set; }

        public Image? Image { get; set; }

        public Thumbnail? Thumbnail { get; set; }

        public Author? Author { get; set; }

        [EnumerableLength(0, 25, "You cannot use more than 25 fields or less than 0 fields")]
        public IEnumerable<EmbedField> Fields { get; set; } = [];

        public static string Type 
        {
            get
            {
                return "rich";
            } 
        }

        public bool IsValid
        {
            get
            {
                return Title != null || Description != null || (Image != null && Image.Url != null) || (Footer != null && Footer.Text != null) || (Author != null && Author.Name != null);
            }
        }
    }

    public class Footer
    {
        [MaxLength(2048)]
        public required string Text { get; set; }

        public string? IconUrl { get; set; }
    }

    public class Image
    {
        public required string Url { get; set; }
    }

    public class Thumbnail : Image { }

    public class Author 
    {
        public required string Name { get; set; }

        public string? Url { get; set; }

        public string? IconUrl { get; set; }
    }

    public class EmbedField
    {
        [MaxLength(256)]
        public required string Name { get; set; }

        [MaxLength(1024)]
        public required string Value { get; set; }
        
        public bool Inline { get; set; }

    }
}
