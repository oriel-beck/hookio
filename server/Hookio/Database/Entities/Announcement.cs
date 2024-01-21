using Hookio.Enunms;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hookio.Database.Entities
{
    [PrimaryKey("Id")]
    public class Announcement
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string GuildId { get; set; }
        public AnnouncementType AnnouncementType { get; set; }
        public string Origin { get; set; }
        // data sent to discord
        public string Message { get; set; }
        // embed data somehow? json?
        
    }
}
