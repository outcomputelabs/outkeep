using Microsoft.EntityFrameworkCore;

namespace Outkeep.Hosting.Standalone.Caching
{
    internal class SqliteCacheRegistryContext : DbContext
    {
        public SqliteCacheRegistryContext(DbContextOptions<SqliteCacheRegistryContext> options)
            : base(options)
        {
        }

        public DbSet<SqliteCacheRegistryEntity> CacheRegistryEntities { get; set; } = null!;
    }
}