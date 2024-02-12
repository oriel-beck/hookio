using Hookio.Enunms;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Hookio.Database.Entities
{
    [PrimaryKey("Id")]
    [Index("GuildId")]
    public class Subscription
    {
        [Key]
        public int Id { get; set; }
        public ulong GuildId { get; set; }
        public SubscriptionType SubscriptionType { get; set; }
        public string WebhookUrl { get; set; }
        // The name/identifier of the channel (yt, twitch, kick, etc) the announcement is linked to
        public string ChannelId { get; set; }
        // data sent to discord
    }
}
