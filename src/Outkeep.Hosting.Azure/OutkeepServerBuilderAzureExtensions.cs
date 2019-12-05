namespace Outkeep.Hosting
{
    public static class OutkeepServerBuilderAzureExtensions
    {
        public static IOutkeepServerBuilder UseAzure(this IOutkeepServerBuilder builder)
        {
            return builder.ConfigureServices(services =>
            {
            });
        }
    }
}