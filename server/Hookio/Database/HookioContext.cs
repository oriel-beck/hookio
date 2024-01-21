using Hookio.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hookio.Database
{
    public class HookioContext : DbContext
    {
        public DbSet<User> users {  get; set; }
        public DbSet<Announcement> announcements { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    => optionsBuilder.UseNpgsql("Host=http://localhost:5432;Database=hookio;Username=hookio;Password=admin");
    }
}
