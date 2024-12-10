using Hookio.Contracts.Embed;
using Hookio.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Newtonsoft.Json;

namespace Hookio.Data
{
    public class HookioContext(DbContextOptions<HookioContext> options) : DbContext(options)
    {
        public DbSet<Message> Messages { get; set; }

        public DbSet<Subscription> Subscriptions { get; set; }

        public DbSet<RecentAction> RecentActions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Message>()
                .Property(e => e.Embeds)
                .HasConversion(v => JsonConvert.SerializeObject(v), v => JsonConvert.DeserializeObject<List<EmbedRequest>>(v)!,
                    new ValueComparer<List<EmbedRequest>>(
                        (c1, c2) => JsonConvert.SerializeObject(c1) == JsonConvert.SerializeObject(c2),  // Equality check
                        c => c == null ? 0 : JsonConvert.SerializeObject(c).GetHashCode(),               // Hash code generation
                        c => JsonConvert.DeserializeObject<List<EmbedRequest>>(JsonConvert.SerializeObject(c))!
                    )
                );

            // Composite primary key, only 1 message type can exist for every subscription
            modelBuilder.Entity<Message>().HasKey(e => new
            {
                e.SubscriptionId,
                e.Type
            });
        }

        public override int SaveChanges()
        {
            CleanupRecentActions();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            CleanupRecentActions();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void CleanupRecentActions()
        {
            foreach (var subscription in ChangeTracker.Entries<Subscription>()
                .Where(e => e.State == EntityState.Modified || e.State == EntityState.Added)
                .Select(e => e.Entity))
            {
                while (subscription.RecentActions.Count > 10)
                {
                    subscription.RecentActions.RemoveAt(0); // FIFO
                }
            }
        }
    }
}
