using Orleans.Hosting;

namespace Outkeep
{
    public static class OutkeepServerBuilderHostingExtensions
    {
        public static IOutkeepServerBuilder AddCoreServices(this IOutkeepServerBuilder builder)
        {
            return builder
                .ConfigureSilo(silo =>
                {
                    silo.AddOutkeep();
                });
        }
    }
}