using Orleans;
using System;

namespace Outkeep.Interfaces
{
    public class OutkeepClient : IOutkeepClient
    {
        private readonly IGrainFactory factory;

        public OutkeepClient(IGrainFactory factory)
        {
            this.factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        public IDistributedCacheGrain GetCacheGrain(string key) => factory.GetGrain<IDistributedCacheGrain>(key);

        public IPingGrain GetPingGrain(Guid key) => factory.GetGrain<IPingGrain>(key);
    }
}