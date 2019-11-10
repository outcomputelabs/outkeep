using Microsoft.Extensions.Options;
using Outkeep.Implementations;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCacheGrainOptions(this IServiceCollection services)
        {
            return services
                .AddSingleton(provider => provider.GetService<IOptions<CacheGrainOptions>>().Value)
                .AddSingleton<CacheOptionsValidator>();
        }
    }
}