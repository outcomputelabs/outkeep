using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Outkeep.Client
{
    public static class OutkeepDistributedCacheServiceCollectionExtensions
    {
        public static IServiceCollection AddOutkeepDistributedCache(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            return services.AddSingleton<IDistributedCache, OutkeepDistributedCache>();
        }
    }
}