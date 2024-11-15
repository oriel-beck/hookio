using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Hookio.Data
{
    public class HookioContextFactory : IDesignTimeDbContextFactory<HookioContext>
    {
        public HookioContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<HookioContext>();
            // replace this with whatever connection string you need to run, then remove this
            optionsBuilder.UseNpgsql("Server=127.0.0.1;Database=hookio;Port=5432;User Id=postgres;Password=very_secure_password;");
            return new HookioContext(optionsBuilder.Options);
        }
    }
}
