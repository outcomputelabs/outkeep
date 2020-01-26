using Outkeep.Governance;
using Outkeep.Grains;

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