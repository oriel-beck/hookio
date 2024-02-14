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
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Subscription>()
                .HasOne(s => s.Message)
                .WithOne(m => m.Subscription)
                .HasForeignKey<Message>(m => m.SubscriptionId);

            modelBuilder.Entity<Message>()
                .HasMany(m => m.Embeds)
                .WithOne(e => e.Message)
                .HasForeignKey(e => e.MessageId);

            modelBuilder.Entity<Embed>()
                .HasMany(e => e.Fields)
                .WithOne(f => f.Embed)
                .HasForeignKey(f => f.EmbedId);
        }
    }
}
