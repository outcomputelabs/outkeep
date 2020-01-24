using Microsoft.Extensions.DependencyInjection;

namespace Outkeep.Core
{
    public static class OutkeepServerBuilderExtensions
    {
        public static IOutkeepServerBuilder AddCoreServices(this IOutkeepServerBuilder builder)
        {
            return builder.ConfigureServices(services =>
            {
                services.AddCoreServices();
            });
        }
    }
}