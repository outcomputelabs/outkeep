using Outkeep.Interfaces;
using System;
using System.Diagnostics.Contracts;

namespace Orleans
{
    public static class GrainFactoryExtensions
    {
        public static ICacheGrain GetCacheGrain(this IGrainFactory factory, string key)
        {
            Contract.Requires(factory != null);
            Contract.Requires(key != null);

            return factory.GetGrain<ICacheGrain>(key);
        }

        public static IEchoGrain GetEchoGrain(this IGrainFactory factory)
        {
            Contract.Requires(factory != null);

            return factory.GetGrain<IEchoGrain>(Guid.Empty);
        }
    }
}