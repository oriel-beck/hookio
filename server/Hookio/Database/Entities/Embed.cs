﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hookio.Database.Entities
{
    public class Embed
    {
        [Key]
        public int Id { get; set; }
        public int Index { get; set; }
        public string? Description { get; set; }
        public string? Title { get; set; }
        public string? TitleUrl { get; set; }
        public string? Color { get; set; }
        public string? Image { get; set; }
        public string? Author { get; set; }
        public string? AuthorUrl { get; set; }
        public string? AuthorIcon { get; set; }
        public string? Footer { get; set; }
        public string? FooterIcon { get; set; }
        public string? Thumbnail { get; set; }
        public bool AddTimestamp { get; set; }
        [ForeignKey("Message")]
        public int MessageId { get; set; }
        public Message Message { get; set; }
        public List<EmbedField> Fields { get; set; }
    }
}
