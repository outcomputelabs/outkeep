using Outkeep.Interfaces;
using System;

namespace Outkeep.Client
{
    public interface IOutkeepClient
    {
        Guid ActivityId { get; }

        IEchoGrain GetEchoGrain();

        IDistributedCacheGrain GetCacheGrain(string key);
    }
}