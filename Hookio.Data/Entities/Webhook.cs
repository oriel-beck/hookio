using Microsoft.EntityFrameworkCore;

namespace Hookio.Data.Entities
{
    [PrimaryKey(nameof(Id))]
    public class Webhook
    {
        public required ulong Id { get; set; }

        public required string Token { get; set; }
    }
}
