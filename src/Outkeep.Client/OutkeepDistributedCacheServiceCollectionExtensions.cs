using Microsoft.Extensions.Caching.Distributed;
using Outkeep.Client;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class OutkeepDistributedCacheServiceCollectionExtensions
    {
        public static IServiceCollection AddOutkeepDistributedCache(this IServiceCollection services, Action<OutkeepDistributedCacheClientOptions> configure)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            return services
                .AddSingleton<IDistributedCache, OutkeepDistributedCache>()
                .Configure(configure);
        }
    }
}