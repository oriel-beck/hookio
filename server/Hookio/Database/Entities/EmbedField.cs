using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hookio.Database.Entities
{
    public class EmbedField
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Value { get; set; }
        public bool Inline { get; set; }
        [ForeignKey("Embed")]
        public int EmbedId { get; set; }
        public Embed Embed { get; set; }
    }
}
