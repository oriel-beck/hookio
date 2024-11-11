using Hookio.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hookio.Data.Entities
{
    [PrimaryKey(nameof(Id))]
    [Index(nameof(GuildId))]
    public class Subscription
    {
        [Key]
        public int Id { get; set; }

        public required ulong GuildId { get; set; }

        public required SubscriptionType SubscriptionType { get; set; }

        [ForeignKey(nameof(Webhook))]
        public required ulong WebhookId { get; set; }

        public Webhook? Webhook { get; set; }

        public IEnumerable<Message>? Messages { get; set; } = [];
    }
}
