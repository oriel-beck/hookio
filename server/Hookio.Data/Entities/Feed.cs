using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Hookio.Database.Entities
{
    [Index(nameof(Url), IsUnique = true)]
    public class Feed
    {
        [Key]
        public int Id { get; set; }
        public required string Url { get; set; }
        public bool Disabled { get; set; }
        public List<Subscription> Subscriptions { get; set; } = [];
        public DateTime? LastPublishedAt { get; set; }
        public string? LastId { get; set; }
    }
}
