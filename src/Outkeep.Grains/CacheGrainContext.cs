using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;

namespace Outkeep.Grains
{
    /// <summary>
    /// This class groups together static dependencies for the cache grain in order to reduce memory footprint for each instance.
    /// </summary>
    internal class CacheGrainContext : ICacheGrainContext
    {
        public CacheGrainContext(ILogger<CacheGrain> logger, IOptions<CacheGrainOptions> options)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            Options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        }

        public ILogger Logger { get; }

        public CacheGrainOptions Options { get; }
    }
}