using Microsoft.Extensions.DependencyInjection;

namespace Outkeep.Caching
{
    public static class CacheActivationContextServiceCollectionExtensions
    {
        public static IServiceCollection AddCacheActivationContext(this IServiceCollection services)
        {
            return services.AddSingleton<ICacheActivationContext, CacheActivationContext>();
        }
    }
}