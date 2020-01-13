using Outkeep.Core;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionOutkeepStorageExtensions
    {
        public static IServiceCollection AddSystemClock(this IServiceCollection services)
        {
            return services.AddSingleton<ISystemClock, SystemClock>();
        }
    }
}