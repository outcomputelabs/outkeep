using Outkeep.Interfaces;
using System;

namespace Orleans
{
    public static class GrainFactoryExtensions
    {
        public static ICacheGrain GetCacheGrain(this IGrainFactory factory, string key)
        {
            if (factory == null) throw new ArgumentNullException(nameof(factory));
            if (key == null) throw new ArgumentNullException(nameof(key));

            return factory.GetGrain<ICacheGrain>(key);
        }

        public static IEchoGrain GetEchoGrain(this IGrainFactory factory)
        {
            if (factory == null) throw new ArgumentNullException(nameof(factory));

            return factory.GetGrain<IEchoGrain>(Guid.Empty);
        }
    }
}