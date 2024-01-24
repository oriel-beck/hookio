using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Hookio.Database.Entities
{
    [PrimaryKey("Id")]
    public class User
    {
        [Required]
        public string Id { get; set; }
        [Required]
        public string AccessToken { get; set; }
        [Required]
        public string RefreshToken { get; set; }
        [Required]
        public DateTimeOffset ExpireAt { get; set; }
    }
}
