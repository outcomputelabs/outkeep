using Outkeep.Caching.Memory;
using System;

namespace Orleans
{
    internal static class MemoryCacheRegistryGrainFactoryExtensions
    {
        public static IMemoryCacheRegistryGrain GetMemoryCacheRegistryGrain(this IGrainFactory factory)
        {
            return factory.GetGrain<IMemoryCacheRegistryGrain>(Guid.Empty);
        }
    }
}