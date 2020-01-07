using Microsoft.Extensions.DependencyInjection;

namespace Outkeep.Core.Caching
{
    /// <summary>
    /// Extensions for adding <see cref="CacheDirector"/> to <see cref="IServiceCollection"/>.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds a <see cref="CacheDirector"/> to the <see cref="IServiceCollection"/> as an <see cref="ICacheDirector"/>.
        /// </summary>
        public static IServiceCollection AddCacheDirector(this IServiceCollection services)
        {
            return services
                .AddSingleton<ICacheDirector, CacheDirector>()
                .AddOptions<CacheDirectorOptions>()
                .ValidateDataAnnotations()
                .Services;
        }
    }
}