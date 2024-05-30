using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        [Required]
        public string Email { get; set; }
        // TODO: implement premium level (tiers in Patreon)
        public DateTimeOffset? PremiumExpires { get; set; }
        [NotMapped]
        public bool Premium
        {
            get
            {
                return PremiumExpires is not null && PremiumExpires > DateTimeOffset.UtcNow;
            }
        }
    }
}
