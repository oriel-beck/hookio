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
            // when field is deleted, set all subscription fields to null
            // fields are deleted when there are 0 enabled subscriptions attached to them
            modelBuilder
                .Entity<Feed>()
                .HasMany(f => f.Subscriptions)
                .WithOne(s => s.Feed)
                .OnDelete(DeleteBehavior.SetNull);

            // when a subscription is deleted, delete all events
            // subscription can only be deleted when a user requests it
            modelBuilder
                .Entity<Subscription>()
                .HasMany(s => s.Events)
                .WithOne(e => e.Subscription)
                .OnDelete(DeleteBehavior.Cascade);

            // when an event is deleted, delete the message
            // event can only be deleted when a subscription is deleted
            modelBuilder
                .Entity<Event>()
                .HasOne(e => e.Message)
                .WithOne(m => m.Event)
                .OnDelete(DeleteBehavior.Cascade);

            // when a message is deleted, delete all embeds
            // message can only be deleted when an event is deleted
            modelBuilder
                .Entity<Message>()
                .HasMany(m => m.Embeds)
                .WithOne(e => e.Message)
                .OnDelete(DeleteBehavior.Cascade);

            // when an embed is deleted, delete all fields
            // embed can only be deleted when a message is deleted
            modelBuilder
                .Entity<Embed>()
                .HasMany(e => e.Fields)
                .WithOne(f => f.Embed)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
