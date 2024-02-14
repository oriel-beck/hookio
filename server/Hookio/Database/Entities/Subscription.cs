using Hookio.Enunms;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hookio.Database.Entities
{
    [PrimaryKey("Id")]
    [Index("GuildId")]
    [Index("SubscriptionType")]
    public class Subscription
    {
        [Key]
        public int Id { get; set; }
        public ulong GuildId { get; set; }
        public SubscriptionType SubscriptionType { get; set; }
        public string WebhookUrl { get; set; }
        public string ChannelId { get; set; }
        public int MessageId { get; set; }
        public Message? Message { get; set; }
    }

}
