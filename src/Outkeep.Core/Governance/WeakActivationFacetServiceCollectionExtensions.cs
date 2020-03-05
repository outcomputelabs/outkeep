using Orleans.Runtime;
using Outkeep.Governance;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class WeakActivationFacetServiceCollectionExtensions
    {
        public static IServiceCollection AddWeakActivationFacet(this IServiceCollection services)
        {
            return services
                .AddSingleton<IWeakActivationStateFactory, WeakActivationStateFactory>()
                .AddSingleton<IAttributeToFactoryMapper<WeakActivationStateAttribute>, WeakActivationStateAttributeMapper>();
        }
    }
}