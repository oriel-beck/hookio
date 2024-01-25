using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Hookio.Database
{
    public class HookioContextFactory : IDesignTimeDbContextFactory<HookioContext>
    {
        public HookioContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<HookioContext>();
            optionsBuilder.UseNpgsql(Environment.GetEnvironmentVariable("PG_CONNECTION_STRING"));
            return new HookioContext(optionsBuilder.Options);
        }
    }
}
