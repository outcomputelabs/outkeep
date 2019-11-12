using Outkeep.Interfaces;
using System;

namespace Orleans
{
    public static class ClusterClientExtensions
    {
        public static ICacheGrain GetCacheGrain(this IClusterClient client, string key)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));

            return client.GetGrain<ICacheGrain>(key);
        }

        public static IEchoGrain GetEchoGrain(this IClusterClient client)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));

            return client.GetGrain<IEchoGrain>(Guid.Empty);
        }
    }
}