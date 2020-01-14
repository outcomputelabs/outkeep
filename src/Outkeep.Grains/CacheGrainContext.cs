using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Outkeep.Core.Storage;
using System;

namespace Outkeep.Grains
{
    /// <summary>
    /// This class groups together static dependencies for the cache grain in order to reduce memory footprint for each instance.
    /// </summary>
    internal class CacheGrainContext : ICacheGrainContext
    {
        public CacheGrainContext(ILogger<CacheGrain> logger, IOptions<CacheGrainOptions> options, ICacheStorage storage)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            Options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            Storage = storage ?? throw new ArgumentNullException(nameof(storage));
        }

        public ILogger Logger { get; }

        public CacheGrainOptions Options { get; }

        public ICacheStorage Storage { get; }
    }
}