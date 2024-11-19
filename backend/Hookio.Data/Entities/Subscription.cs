using Hookio.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Hookio.Data.Entities
{
    [PrimaryKey(nameof(Id))]
    [Index(nameof(GuildId))]
    public class Subscription
    {
        [Key]
        public int Id { get; set; }

        public required string GuildId { get; set; }

        public required SubscriptionType SubscriptionType { get; set; }

        public required string WebhookUrl { get; set; }

        public List<Message> Messages { get; set; } = [];

        public List<RecentAction> RecentActions { get; set; } = [];

        // This is used for ratelimits as webhooks are ratelimited per channel
        public required string ChannelId { get; set; }
    }
}
