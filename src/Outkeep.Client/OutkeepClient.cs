using Orleans;
using Outkeep.Interfaces;
using System;

namespace Outkeep.Client
{
    internal class OutkeepClient : IOutkeepClient
    {
        private readonly IGrainFactory factory;

        public OutkeepClient(IGrainFactory factory)
        {
            this.factory = factory;
        }

        public IDistributedCacheGrain GetCacheGrain(string key) => factory.GetGrain<IDistributedCacheGrain>(key);

        public IPingGrain GetPingGrain(Guid key) => factory.GetGrain<IPingGrain>(key);
    }
}