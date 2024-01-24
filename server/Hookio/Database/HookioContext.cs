using Hookio.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hookio.Database
{
    public class HookioContext(DbContextOptions<HookioContext> options) : DbContext(options)
    {
        public DbSet<User> Users {  get; set; }
        public DbSet<Announcement> Announcements { get; set; }
    }
}
