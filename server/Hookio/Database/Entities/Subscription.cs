﻿using Hookio.Enunms;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

using System.ComponentModel.DataAnnotations.Schema;

namespace Hookio.Database.Entities
{
    [PrimaryKey("Id")]
    [Index("GuildId")]
    [Index("SubscriptionType")]
    [Index("Url")]
    public class Subscription
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public ulong GuildId { get; set; }
        public SubscriptionType SubscriptionType { get; set; }
        [Required]
        [Column(TypeName = "varchar(200)")]
        public string WebhookUrl { get; set; }
        [Required]
        [Column(TypeName = "varchar(200)")]
        public string Url { get; set; }
        public List<Event> Events { get; set; }
        public bool Disabled { get; set; }
        [Column(TypeName = "varchar(1000)")]
        public string? DisabledReason { get; set; }
    }

}
