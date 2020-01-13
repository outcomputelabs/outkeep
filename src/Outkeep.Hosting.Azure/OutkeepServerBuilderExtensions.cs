namespace Outkeep.Core
{
    public static class OutkeepServerBuilderExtensions
    {
        public static IOutkeepServerBuilder UseAzure(this IOutkeepServerBuilder builder)
        {
            return builder.ConfigureServices(services =>
            {
            });
        }
    }
}