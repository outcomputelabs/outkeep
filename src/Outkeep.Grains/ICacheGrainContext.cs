using Microsoft.Extensions.Logging;

namespace Outkeep.Grains
{
    internal interface ICacheGrainContext
    {
        ILogger Logger { get; }
    }
}