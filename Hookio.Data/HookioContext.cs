using Hookio.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hookio.Data
{
    public class HookioContext(DbContextOptions<HookioContext> options) : DbContext(options)
    {
        public DbSet<Message> Messages { get; set; }

        public DbSet<Subscription> Subscriptions { get; set; }

        public DbSet<User> Users { get; set; }

        public DbSet<Webhook> Webhooks { get; set; }
    }
}
