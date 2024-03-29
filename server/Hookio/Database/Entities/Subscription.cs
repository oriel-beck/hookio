﻿using Hookio.Enunms;
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
        [Required]
        public ulong GuildId { get; set; }
        public SubscriptionType SubscriptionType { get; set; }
        [Required]
        public string WebhookUrl { get; set; }
        [Required]
        public string ChannelId { get; set; }
        public List<Message> Messages { get; set; }
    }

}
