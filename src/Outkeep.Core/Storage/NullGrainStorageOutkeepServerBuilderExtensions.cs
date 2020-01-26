using Microsoft.Extensions.DependencyInjection;

namespace Outkeep.Storage
{
    public static class NullGrainStorageOutkeepServerBuilderExtensions
    {
        public static IOutkeepServerBuilder AddNullGrainStorage(this IOutkeepServerBuilder builder, string name)
        {
            return builder.ConfigureServices(services =>
            {
                services.AddNullGrainStorage(name);
            });
        }

        public static IOutkeepServerBuilder AddNullGrainStorageAsDefault(this IOutkeepServerBuilder builder)
        {
            return builder.ConfigureServices(services =>
            {
                services.AddNullGrainStorageAsDefault();
            });
        }
    }
}