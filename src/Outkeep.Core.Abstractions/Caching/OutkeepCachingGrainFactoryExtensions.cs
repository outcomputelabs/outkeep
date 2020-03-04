using Outkeep.Caching;
using System;

namespace Orleans
{
    public static class OutkeepCachingGrainFactoryExtensions
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
    }
}