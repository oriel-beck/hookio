using Discord;
using System.ComponentModel.DataAnnotations;

namespace Hookio.Database.Entities
{
    public class Message
    {
        [Key]
        public int Id { get; set; }
        public string Content { get; set; }
        public int SubscriptionId { get; set; }
        public Subscription Subscription { get; set; }
        public List<Embed> Embeds { get; set; }
    }
}
