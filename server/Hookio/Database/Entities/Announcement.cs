﻿using Hookio.Enunms;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Hookio.Database.Entities
{
    [PrimaryKey("Id")]
    public class Announcement
    {
        [Key]
        public int Id { get; set; }
        public string GuildId { get; set; }
        public AnnouncementType AnnouncementType { get; set; }
        public string WebhookUrl { get; set; }
        // The name/identifier of the channel (yt, twitch, kick, etc) the announcement is linked to
        public string Origin { get; set; }
        // data sent to discord
        public string Message { get; set; }
        // embed data somehow? json?
    }
}
