using Outkeep.Interfaces;
using System;
using System.Diagnostics.Contracts;

namespace Orleans
{
    public static class ClusterClientExtensions
    {
        public static ICacheGrain GetCacheGrain(this IClusterClient client, string key)
        {
            Contract.Requires(client != null);
            Contract.Requires(key != null);

            return client.GetGrain<ICacheGrain>(key);
        }

        public static IEchoGrain GetEchoGrain(this IClusterClient client)
        {
            Contract.Requires(client != null);

            return client.GetGrain<IEchoGrain>(Guid.Empty);
        }
    }
}