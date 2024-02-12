using Discord;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hookio.Database.Entities
{
    public class Message
    {
        [Key]
        public int Id { get; set; }

        public string? Content { get; set; }
        public Embed[] Embeds { get; set; } = [];
        public int AnnouncementId {  get; set; }
        [ForeignKey("AnnouncementId")]
        public Subscription? Announcement { get; set; }
    }
}
