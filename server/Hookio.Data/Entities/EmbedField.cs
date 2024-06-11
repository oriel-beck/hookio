using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hookio.Database.Entities
{
    public class EmbedField
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public required int Index { get; set; }
        [Required]
        [Column(TypeName = "varchar(256)")]
        public required string Name { get; set; }
        [Required]
        [Column(TypeName = "varchar(1024)")]
        public required string Value { get; set; }
        [Required]
        public bool Inline { get; set; }
        [ForeignKey("Embed")]
        public int EmbedId { get; set; }
        public Embed Embed { get; set; } = default!;
    }
}
