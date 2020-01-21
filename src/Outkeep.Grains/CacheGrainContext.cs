using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans.Timers;
using Outkeep.Core;
using Outkeep.Core.Caching;
using System;

namespace Outkeep.Grains
{
    /// <summary>
    /// This class groups together static dependencies for the cache grain in order to reduce memory footprint for each instance.
    /// </summary>
    public class CacheGrainContext : ICacheGrainContext
    {
        public CacheGrainContext(ILogger<CacheGrainContext> logger, IOptions<CacheGrainOptions> options, ISystemClock clock, ICacheDirector<string> director, ITimerRegistry timerRegistry)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            Options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            Clock = clock ?? throw new ArgumentNullException(nameof(clock));
            Director = director ?? throw new ArgumentNullException(nameof(director));
            TimerRegistry = timerRegistry ?? throw new ArgumentNullException(nameof(timerRegistry));
        }

        public ILogger Logger { get; }

        public CacheGrainOptions Options { get; }

        public ISystemClock Clock { get; }

        public ICacheDirector<string> Director { get; }

        public ITimerRegistry TimerRegistry { get; }
    }
}