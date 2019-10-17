using Orleans;
using Outkeep.Interfaces;
using System;

namespace Outkeep.Client
{
    public class OutkeepClient : IOutkeepClient
    {
        private readonly IGrainFactory factory;

        public OutkeepClient(IGrainFactory factory)
        {
            this.factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        public IPingGrain GetPingGrain(Guid key) => factory.GetGrain<IPingGrain>(key);
    }
}