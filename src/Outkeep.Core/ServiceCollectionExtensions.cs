using Microsoft.Extensions.DependencyInjection;

namespace Outkeep.Core
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSystemClock(this IServiceCollection services)
        {
            return services.AddSingleton<ISystemClock, SystemClock>();
        }
    }
}