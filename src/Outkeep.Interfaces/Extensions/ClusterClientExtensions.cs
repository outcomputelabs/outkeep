using Outkeep.Interfaces;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Orleans
{
    public static class ClusterClientExtensions
    {
        [SuppressMessage("Design", "CA1062:Validate arguments of public methods")]
        public static ICacheGrain GetCacheGrain(this IClusterClient client, string key)
        {
            if (client == null) ThrowClientNull();

            return client.GetGrain<ICacheGrain>(key);

            void ThrowClientNull() => throw new ArgumentNullException(nameof(client));
        }

        [SuppressMessage("Design", "CA1062:Validate arguments of public methods")]
        public static IEchoGrain GetEchoGrain(this IClusterClient client)
        {
            if (client == null) ThrowClientNull();

            return client.GetGrain<IEchoGrain>(Guid.Empty);

            void ThrowClientNull() => throw new ArgumentNullException(nameof(client));
        }
    }
}