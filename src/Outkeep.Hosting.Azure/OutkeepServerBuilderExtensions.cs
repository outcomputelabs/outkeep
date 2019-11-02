using System;

namespace Outkeep.Hosting
{
    public static class OutkeepServerBuilderExtensions
    {
        public static IOutkeepServerBuilder UseAzure(this IOutkeepServerBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.ConfigureServices(services =>
            {
                // todo: use azure clustering here

            });
        }
    }
}