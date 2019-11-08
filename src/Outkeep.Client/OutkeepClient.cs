using Orleans;
using Orleans.Runtime;
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

        public Guid ActivityId
        {
            get
            {
                return RequestContext.ActivityId;
            }
            set
            {
                RequestContext.ActivityId = value;
                RequestContext.PropagateActivityId = true;
            }
        }

        public IDistributedCacheGrain GetCacheGrain(string key) => factory.GetGrain<IDistributedCacheGrain>(key);

        public IEchoGrain GetEchoGrain() => factory.GetGrain<IEchoGrain>(Guid.Empty);
    }
}