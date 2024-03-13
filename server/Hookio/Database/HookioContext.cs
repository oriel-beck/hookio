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
    }
}
