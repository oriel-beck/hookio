using System.ComponentModel.DataAnnotations;

namespace Hookio.Database.Entities
{
    public class Feed
    {
        [Key]
        public int Id { get; set; }
        public required string Url { get; set; }
        public bool Disabled { get; set; }
        public List<Subscription> Subscriptions { get; set; } = default!;
        public DateTime? LastPublishedAt { get; set; }
        public string? LastId { get; set; }
    }
}
