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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Message>()
                .Property(e => e.Embeds)
                .HasConversion(v => JsonConvert.SerializeObject(v), v => JsonConvert.DeserializeObject<List<EmbedRequest>>(v),
                    new ValueComparer<List<EmbedRequest>>(
                        (c1, c2) => JsonConvert.SerializeObject(c1) == JsonConvert.SerializeObject(c2),  // Equality check
                        c => c == null ? 0 : JsonConvert.SerializeObject(c).GetHashCode(),               // Hash code generation
                        c => JsonConvert.DeserializeObject<List<EmbedRequest>>(JsonConvert.SerializeObject(c))!
                    )
                );
        }
    }
}
