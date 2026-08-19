using Hookio.Enums;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hookio.Database.Entities
{
    [PrimaryKey("Id")]
    [Index("GuildId")]
    [Index("SubscriptionType")]
    [Index(nameof(TwitchBroadcasterId))]
    public class Subscription
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public ulong GuildId { get; set; }

        public SubscriptionType SubscriptionType { get; set; }

        [Required]
        [Column(TypeName = "varchar(200)")]
        public required string WebhookUrl { get; set; }

        [Required]
        public ulong WebhookChannel { get; set; }

        public List<Event> Events { get; set; } = [];

        public bool Disabled { get; set; }

        [Column(TypeName = "varchar(1000)")]
        public string? DisabledReason { get; set; }

        [ForeignKey(nameof(Feed))]
        public int? FeedId { get; set; }
        public Feed? Feed { get; set; }

        [Column(TypeName = "varchar(32)")]
        public string? TwitchBroadcasterId { get; set; }

        [Column(TypeName = "varchar(25)")]
        public string? TwitchLogin { get; set; }

        [Column(TypeName = "varchar(200)")]
        public string? TwitchEventSubIds { get; set; }
    }
}
