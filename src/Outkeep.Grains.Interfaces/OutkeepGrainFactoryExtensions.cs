using Outkeep.Grains;
using Outkeep.Grains.Caching;
using System;

namespace Orleans
{
    /// <summary>
    /// Quality-of-life extension methods for <see cref="IGrainFactory"/>.
    /// </summary>
    public static class OutkeepGrainFactoryExtensions
    {
        /// <summary>
        /// Gets a proxy to the <see cref="ICacheGrain"/> with the given key.
        /// </summary>
        public static ICacheGrain GetCacheGrain(this IGrainFactory factory, string key)
        {
            if (factory == null) throw new ArgumentNullException(nameof(factory));
            if (key == null) throw new ArgumentNullException(nameof(key));

            return factory.GetGrain<ICacheGrain>(key);
        }

        /// <summary>
        /// Gets a proxy to an <see cref="IEchoGrain"/>.
        /// </summary>
        public static IEchoGrain GetEchoGrain(this IGrainFactory factory)
        {
            if (factory == null) throw new ArgumentNullException(nameof(factory));

            return factory.GetGrain<IEchoGrain>(Guid.Empty);
        }
    }
}