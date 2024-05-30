using Hookio.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hookio.Database
{
    public class HookioContext(DbContextOptions<HookioContext> options) : DbContext(options)
    {
        public DbSet<User> Users {  get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Embed> Embeds { get; set; }
        public DbSet<EmbedField> EmbedFields { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Feed> Feeds { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<Feed>()
                .HasMany(e => e.Subscriptions)
                .WithOne(e => e.Feed)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
