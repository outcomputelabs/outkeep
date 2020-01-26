using Outkeep.Grains;
using Outkeep.Grains.Governance;

namespace Orleans.Hosting
{
    public static class GrainControlExtensionSiloBuilderExtensions
    {
        public static ISiloBuilder AddGrainControl(this ISiloBuilder builder)
        {
            return builder.AddGrainExtension<IGrainControlExtension, GrainControlExtension>();
        }
    }
}