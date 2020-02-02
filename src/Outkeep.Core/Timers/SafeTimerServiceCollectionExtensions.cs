using Outkeep.Timers;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SafeTimerServiceCollectionExtensions
    {
        public static IServiceCollection AddSafeTimer(this IServiceCollection services)
        {
            return services.AddSingleton<ISafeTimerFactory, SafeTimerFactory>();
        }
    }
}