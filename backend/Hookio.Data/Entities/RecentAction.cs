using Hookio.Shared.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hookio.Data.Entities
{
    public class RecentAction
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(Subscription))]
        public required int SubscriptionId { get; set; }
    
        public Subscription? Subscription { get; set; }

        public required RecentActionType Type { get; set; }

        public required MessageAction Action { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        // This is only for me tbh
        public string? Error { get; set; }
    }
}
