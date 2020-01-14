using Microsoft.Extensions.Logging;
using Outkeep.Core.Storage;

namespace Outkeep.Grains
{
    internal interface ICacheGrainContext
    {
        ILogger Logger { get; }
        CacheGrainOptions Options { get; }
        ICacheStorage Storage { get; }
    }
}