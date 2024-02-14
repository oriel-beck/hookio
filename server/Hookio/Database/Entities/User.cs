using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Hookio.Database.Entities
{
    [PrimaryKey("Id")]
    public class User
    {
        [Required]
        public ulong Id { get; set; }
        [Required]
        public string AccessToken { get; set; }
        [Required]
        public string RefreshToken { get; set; }
        [Required]
        public DateTimeOffset ExpireAt { get; set; }
        public bool Premium
        {
            get
            {
                return PremiumExpires is not null && PremiumExpires > DateTimeOffset.UtcNow;
            }
        }
        public DateTimeOffset? PremiumExpires { get; set; }
    }
}
