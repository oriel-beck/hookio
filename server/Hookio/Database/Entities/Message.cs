﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hookio.Database.Entities
{
    public class Message
    {
        [Key]
        public int Id { get; set; }
        public string Content { get; set; }
        public List<Embed> Embeds { get; set; }
        public string? webhookUsername { get; set; }
        public string? webhookAvatar { get; set; }
        [ForeignKey(nameof(Event))]
        public int EventId { get; set; }
        public Event Event { get; set; }
        // TODO: add the event to the message so it will only get the message that is required for this event
        // YT events: new, edit, (delete?, can call it end to match but eh)
        // Twitch events: new, edit, end
    }
}
