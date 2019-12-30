using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Outkeep.Core.Caching
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection TryAddCacheDirector(this IServiceCollection services)
        {
            services.TryAddSingleton<ICacheDirector, CacheDirector>();
            services.TryAddSingleton<IValidateOptions<CacheDirectorOptions>, CacheDirectorOptionsValidator>();
            return services;
        }
    }
}