using Outkeep.Governance;

namespace Orleans.Hosting
{
    public static class WeakActivationGrainExtensionSiloBuilderExtensions
    {
        public static ISiloBuilder AddWeakActivationExtension(this ISiloBuilder builder)
        {
            return builder
                .AddGrainExtension<IWeakActivationExtension, WeakActivationGrainExtension>();
        }
    }
}