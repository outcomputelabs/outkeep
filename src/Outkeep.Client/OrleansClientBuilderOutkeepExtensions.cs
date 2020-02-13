using Outkeep.Caching;

namespace Orleans
{
    public static class OrleansClientBuilderOutkeepExtensions
    {
        /// <summary>
        /// Adds Outkeep grain interfaces to the specified <see cref="IClientBuilder"/>.
        /// </summary>
        public static IClientBuilder AddOutkeep(this IClientBuilder builder)
        {
            return builder.ConfigureApplicationParts(manager => manager.AddApplicationPart(typeof(ICacheGrain).Assembly).WithReferences());
        }
    }
}