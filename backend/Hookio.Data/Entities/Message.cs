using Hookio.Contracts.Embed;
using Hookio.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hookio.Data.Entities
{
    /// <summary>
    /// Message holds the message data to send when this message gets triggered and the action that should be done<br/>
    /// Note: messages of action "delete" can be empty, their content and embeds is ignored
    /// </summary>
    [PrimaryKey(nameof(Id))]
    public class Message
    {
        [Key]
        public int Id { get; set; }

        // What do do when this message type gets triggered (null on initial creation)
        public required MessageAction? Action { get; set; }

        // The event this message should be triggered for
        public required MessageType Type { get; set; }

        public string? Content { get; set; }

        public List<EmbedRequest> Embeds { get; set; } = [];

        [ForeignKey(nameof(Subscription))]
        public required int SubscriptionId { get; set; }

        public Subscription? Subscription { get; set; }
    }
}
