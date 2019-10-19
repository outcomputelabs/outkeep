using Orleans;
using Orleans.Concurrency;
using Orleans.Timers;
using Outkeep.Interfaces;
using System;
using System.Threading.Tasks;

namespace Outkeep.Implementations
{
    public class DistributedCacheGrain : Grain, IDistributedCacheGrain
    {
        private readonly ITimerRegistry timerRegistry;

        public DistributedCacheGrain(ITimerRegistry timerRegistry)
        {
            this.timerRegistry = timerRegistry;
        }

        private Task<Immutable<byte[]>> value;
        private DateTimeOffset? absoluteExpiration;
        private TimeSpan? absoluteExpirationRelativeToNow;
        private TimeSpan? slidingExpiration;

        public override Task OnActivateAsync()
        {
            timerRegistry.RegisterTimer(this, TickExpirationPolicy, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
        }

        private Task TickExpirationPolicy(object state)
        {
            return Task.CompletedTask;
        }

        public Task<Immutable<byte[]>> GetAsync() => value;

        public Task RemoveAsync()
        {
            DeactivateOnIdle();
            return Task.CompletedTask;
        }

        public Task SetAsync(Immutable<byte[]> value, DateTimeOffset? absoluteExpiration, TimeSpan? absoluteExpirationRelativeToNow, TimeSpan? slidingExpiration)
        {
            this.value = Task.FromResult(value);
            this.absoluteExpiration = absoluteExpiration;
            this.absoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow;
            this.slidingExpiration = slidingExpiration;

            return Task.CompletedTask;
        }
    }
}