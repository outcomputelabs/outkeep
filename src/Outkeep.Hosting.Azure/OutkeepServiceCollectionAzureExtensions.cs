using Microsoft.Extensions.DependencyInjection;

namespace Outkeep.Hosting.Azure
{
    public static class OutkeepServiceCollectionAzureExtensions
    {
        public static IServiceCollection AddOutkeepAzureClustering(this IServiceCollection services)
        {
            return services;
        }
    }
}