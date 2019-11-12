using Microsoft.Extensions.Options;
using Orleans;
using Orleans.Concurrency;
using Orleans.Timers;
using Outkeep.Interfaces;
using System;
using System.Threading.Tasks;

namespace Outkeep.Grains
{
    internal class CacheGrain : Grain, ICacheGrain
    {
        private readonly CacheGrainOptions options;
        private readonly ITimerRegistry timerRegistry;

        public CacheGrain(IOptions<CacheGrainOptions> options, ITimerRegistry timerRegistry)
        {
            this.options = options?.Value;
            this.timerRegistry = timerRegistry;
        }

        private Task<Immutable<byte[]>> value;
        private DateTimeOffset accessed;
        private DateTimeOffset? absoluteExpiration;
        private TimeSpan? slidingExpiration;

        public override Task OnActivateAsync()
        {
            timerRegistry.RegisterTimer(this, TickExpirationPolicy, null, options.ExpirationPolicyEvaluationPeriod, options.ExpirationPolicyEvaluationPeriod);
            return Task.CompletedTask;
        }

        private Task TickExpirationPolicy(object state)
        {
            var now = DateTimeOffset.UtcNow;

            if (absoluteExpiration.HasValue && absoluteExpiration.Value <= now ||
                slidingExpiration.HasValue && accessed.Add(slidingExpiration.Value) <= now)
            {
                DeactivateOnIdle();
            }

            return Task.CompletedTask;
        }

        public Task<Immutable<byte[]>> GetAsync()
        {
            accessed = DateTimeOffset.UtcNow;
            return value;
        }

        public Task RemoveAsync()
        {
            DeactivateOnIdle();
            return Task.CompletedTask;
        }

        public Task SetAsync(Immutable<byte[]> value, DateTimeOffset? absoluteExpiration, TimeSpan? slidingExpiration)
        {
            this.value = Task.FromResult(value);
            this.absoluteExpiration = absoluteExpiration;
            this.slidingExpiration = slidingExpiration;

            accessed = DateTimeOffset.UtcNow;

            return Task.CompletedTask;
        }

        public Task RefreshAsync()
        {
            accessed = DateTimeOffset.UtcNow;
            return Task.CompletedTask;
        }
    }
}