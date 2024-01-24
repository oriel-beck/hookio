using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Hookio.Database
{
    public class HookioContextFactory : IDesignTimeDbContextFactory<HookioContext>
    {
        public HookioContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<HookioContext>();
            optionsBuilder.UseNpgsql("Server=127.0.0.1;Database=hookio;Port=5432;User Id=postgres;Password=admin;");
            return new HookioContext(optionsBuilder.Options);
        }
    }
}
