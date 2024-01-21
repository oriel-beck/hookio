using Microsoft.EntityFrameworkCore;

namespace Hookio.Database.Entities
{
    [PrimaryKey("Id")]
    public class User
    {
        public string Id { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime ExpireAt { get; set; }
    }
}
