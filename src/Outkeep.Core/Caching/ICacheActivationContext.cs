using Outkeep.Time;

namespace Outkeep.Caching
{
    /// <summary>
    /// Groups shared services to reduce memory footprint of cache grains.
    /// </summary>
    internal interface ICacheActivationContext
    {
        CacheOptions Options { get; }

        ISystemClock Clock { get; }

        ICacheRegistry CacheRegistry { get; }
    }
}