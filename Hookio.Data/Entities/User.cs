using Microsoft.EntityFrameworkCore;

namespace Hookio.Data.Entities
{
    [PrimaryKey(nameof(Id))]
    public class User
    {
        public ulong Id { get; set; }

        public required string AccessToken { get; set; }

        public required string RefreshToken { get; set; }

        public required DateTimeOffset ExpireAt { get; set; }

        // TODO: implement premium via another entity
    }
}
