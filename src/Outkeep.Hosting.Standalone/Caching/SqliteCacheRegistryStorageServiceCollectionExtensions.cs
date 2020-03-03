using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Outkeep.Hosting.Standalone.Caching;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SqliteCacheRegistryStorageServiceCollectionExtensions
    {
        public static IServiceCollection AddSqliteCacheRegistryStorage(this IServiceCollection services, Action<SqliteCacheRegistryStorageOptions> configure)
        {
            return services
                .AddOptions<SqliteCacheRegistryStorageOptions>()
                .Configure(configure)
                .ValidateDataAnnotations()
                .Services
                .AddDbContextPool<SqliteCacheRegistryContext>((sp, options) =>
                {
                    var storageOptions = sp.GetRequiredService<IOptions<SqliteCacheRegistryStorageOptions>>().Value;

                    options.UseSqlite(storageOptions.ConnectionString, sqliteOptions =>
                    {
                        sqliteOptions
                            .MigrationsHistoryTable(storageOptions.MigrationsHistoryTable, storageOptions.MigrationsHistoryTableSchema);
                    });
                });
        }
    }
}