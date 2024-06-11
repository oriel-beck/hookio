using Hookio.Enunms;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hookio.Database.Entities
{
    public class Event
    {
        [Key]
        public int Id { get; set; }
        public required EventType Type { get; set; }
        public Message Message { get; set; } = default!;
        [ForeignKey("Subscription")]
        public int SubscriptionId { get; set; }
        public Subscription Subscription { get; set; } = default!;
    }
}
