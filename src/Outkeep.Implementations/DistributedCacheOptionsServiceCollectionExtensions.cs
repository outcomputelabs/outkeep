using Microsoft.Extensions.Options;
using Outkeep.Implementations;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DistributedCacheOptionsServiceCollectionExtensions
    {
        public static IServiceCollection AddDistributedCacheOptions(this IServiceCollection services)
        {
            return services
                .AddSingleton(provider => provider.GetService<IOptions<DistributedCacheOptions>>().Value)
                .AddSingleton<DistributedCacheOptionsValidator>();
        }
    }
}