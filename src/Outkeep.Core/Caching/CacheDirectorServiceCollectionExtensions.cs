using Microsoft.Extensions.DependencyInjection;
using System;

namespace Outkeep.Core.Caching
{
    /// <summary>
    /// Extensions for adding <see cref="CacheDirector"/> to <see cref="IServiceCollection"/>.
    /// </summary>
    public static class CacheDirectorServiceCollectionExtensions
    {
        public static IServiceCollection AddCacheDirector(this IServiceCollection services, Action<CacheDirectorOptions> configure)
        {
            return services.AddCacheDirector(null, configure);
        }

        public static IServiceCollection AddCacheDirector(this IServiceCollection services, string? name, Action<CacheDirectorOptions> configure)
        {
            return services
                .AddSingleton<CacheDirector>()
                .AddSingleton<CacheDirectorOptionsValidator>()
                .Configure(name, configure);
        }
    }
}