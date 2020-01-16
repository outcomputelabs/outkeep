using Microsoft.Extensions.DependencyInjection;

namespace Outkeep.Grains
{
    public static class CacheGrainContextServiceCollectionExtensions
    {
        public static IServiceCollection AddCacheGrainContext(this IServiceCollection services)
        {
            return services.AddSingleton<ICacheGrainContext, CacheGrainContext>();
        }
    }
}