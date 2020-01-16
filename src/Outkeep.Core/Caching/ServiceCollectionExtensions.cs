using Outkeep.Core.Caching;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extensions for adding <see cref="CacheDirector"/> to <see cref="IServiceCollection"/>.
    /// </summary>
    public static class ServiceCollectionStorageExtensions
    {
        public static IServiceCollection AddCacheDirector(this IServiceCollection services)
        {
            return services.AddCacheDirector(options =>
            {
            });
        }

        /// <summary>
        /// Adds a <see cref="CacheDirector"/> to the <see cref="IServiceCollection"/> as an <see cref="ICacheDirector"/>.
        /// </summary>
        public static IServiceCollection AddCacheDirector(this IServiceCollection services, Action<CacheOptions> configure)
        {
            return services
                .AddSingleton(typeof(ICacheDirector<>), typeof(CacheDirector<>))
                .AddOptions<CacheOptions>()
                .Configure(configure)
                .ValidateDataAnnotations()
                .Services;
        }
    }
}