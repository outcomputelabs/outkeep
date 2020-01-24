using Orleans.Hosting;

namespace Outkeep.Core
{
    public static class GrainControlExtensionOutkeepServerBuilderExtensions
    {
        public static IOutkeepServerBuilder AddGrainControl(this IOutkeepServerBuilder builder)
        {
            return builder.ConfigureSilo(silo =>
            {
                silo.AddGrainControl();
            });
        }
    }
}