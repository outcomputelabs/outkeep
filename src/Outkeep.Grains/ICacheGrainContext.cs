using Microsoft.Extensions.Logging;
using Orleans.Timers;
using Outkeep.Core;
using Outkeep.Core.Caching;

namespace Outkeep.Grains
{
    public interface ICacheGrainContext
    {
        ILogger Logger { get; }
        CacheGrainOptions Options { get; }
        ISystemClock Clock { get; }
        ICacheDirector<string> Director { get; }
        ITimerRegistry TimerRegistry { get; }
    }
}