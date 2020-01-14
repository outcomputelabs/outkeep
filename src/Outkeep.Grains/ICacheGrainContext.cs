using Microsoft.Extensions.Logging;
using Outkeep.Core;
using Outkeep.Core.Caching;
using Outkeep.Core.Storage;

namespace Outkeep.Grains
{
    internal interface ICacheGrainContext
    {
        ILogger Logger { get; }
        CacheGrainOptions Options { get; }
        ICacheStorage Storage { get; }
        ISystemClock Clock { get; }
        ICacheDirector Director { get; }
    }
}