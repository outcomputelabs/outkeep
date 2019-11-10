using Outkeep.Grains;
using System;
using System.Diagnostics.Contracts;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCacheGrainOptions(this IServiceCollection services, Action<CacheGrainOptions> configure)
        {
            Contract.Requires(services != null);
            Contract.Requires(configure != null);

            return services
                .Configure(configure)
                .AddSingleton<CacheOptionsValidator>();
        }
    }
}