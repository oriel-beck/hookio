using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hookio.Data.Entities
{
    [PrimaryKey(nameof(Id))]
    public class Message
    {
        [Key]
        public int Id { get; set; }

        public string? Content { get; set; }

        // Stringified JSON of an embed
        public string? Embed {  get; set; }

        [ForeignKey(nameof(Subscription))]
        public required int SubscriptionId { get; set; }

        public Subscription? Subscription { get; set; }
    }
}
