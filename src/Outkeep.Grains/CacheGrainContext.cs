using Microsoft.Extensions.Logging;
using System;

namespace Outkeep.Grains
{
    /// <summary>
    /// This class groups together static dependencies for the cache grain in order to reduce memory footprint for each instance.
    /// </summary>
    internal class CacheGrainContext : ICacheGrainContext
    {
        public CacheGrainContext(ILogger<CacheGrain> logger)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public ILogger Logger { get; }
    }
}