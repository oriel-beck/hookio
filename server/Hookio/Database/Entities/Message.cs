using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hookio.Database.Entities
{
    public class Message
    {
        [Key]
        public int Id { get; set; }
        public string Content { get; set; }
        public List<Embed> Embeds { get; set; }
        public string? WebhookUsername { get; set; }
        public string? WebhookAvatar { get; set; }
        [ForeignKey(nameof(Event))]
        public int EventId { get; set; }
        public Event Event { get; set; }
    }
}
