using Microsoft.Extensions.DependencyInjection;

namespace Outkeep
{
    public static class SafeTimerServiceCollectionExtensions
    {
        public static IServiceCollection AddSafeTimer(this IServiceCollection services)
        {
            return services.AddSingleton<ISafeTimerFactory, SafeTimerFactory>();
        }
    }
}