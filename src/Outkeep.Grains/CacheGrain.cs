using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans;
using Orleans.Concurrency;
using Orleans.Core;
using Orleans.Timers;
using Outkeep.Core;
using Outkeep.Interfaces;
using System;
using System.Threading.Tasks;

namespace Outkeep.Grains
{
    internal class CacheGrain : Grain, ICacheGrain, IIncomingGrainCallFilter
    {
        private readonly CacheGrainOptions options;
        private readonly ILogger<CacheGrain> logger;
        private readonly ITimerRegistry timerRegistry;
        private readonly ICacheStorage storage;
        private readonly ISystemClock clock;
        private readonly IGrainIdentity identity;

        public CacheGrain(IOptions<CacheGrainOptions> options, ILogger<CacheGrain> logger, ITimerRegistry timerRegistry, ICacheStorage storage, ISystemClock clock, IGrainIdentity identity)
        {
            this.options = options?.Value;
            this.logger = logger;
            this.timerRegistry = timerRegistry;
            this.storage = storage;
            this.clock = clock;
            this.identity = identity;
        }

        private Task<Immutable<byte[]>> valueAsTask;
        private DateTimeOffset? accessed;
        private DateTimeOffset? absoluteExpiration;
        private TimeSpan? slidingExpiration;

        public override Task OnActivateAsync()
        {
            // start an expiry policy evaluation timer
            timerRegistry.RegisterTimer(this, TickExpirationPolicy, null, options.ExpirationPolicyEvaluationPeriod, options.ExpirationPolicyEvaluationPeriod);

            return Task.CompletedTask;
        }

        private async Task TickExpirationPolicy(object state)
        {
            var now = clock.UtcNow;

            if ((absoluteExpiration.HasValue && absoluteExpiration.Value <= now) ||
                (slidingExpiration.HasValue && accessed.GetValueOrDefault(DateTimeOffset.MinValue).Add(slidingExpiration.Value) <= now))
            {
                await storage.ClearAsync(identity.PrimaryKeyString).ConfigureAwait(true);

                DeactivateOnIdle();
            }
        }

        /// <summary>
        /// Gets the cached value.
        /// Only loads from storage if the value is not yet in memory.
        /// E.g. a SET operation on an empty grain will not trigger a load attempt from storage as the built-in persistent interface would.
        /// </summary>
        /// <returns></returns>
        public Task<Immutable<byte[]>> GetAsync()
        {
            // quick path when value is already in memory
            // we assume this method will be called far more often than the set method
            // therefore we have pre-built the task object to reduce allocations over time
            if (accessed.HasValue)
            {
                accessed = clock.UtcNow;
                return valueAsTask;
            }

            // load value from storage only if not in memory yet
            // we assume this method will rarely be called
            return LoadAsync();
        }

        /// <summary>
        /// Deferred load from storage for when the value is not yet in memory.
        /// </summary>
        private async Task<Immutable<byte[]>> LoadAsync()
        {
            // attempt to read the value from storage as a best effort
            var saved = await storage.TryReadAsync(identity.PrimaryKeyString).ConfigureAwait(true);
            if (saved.HasValue)
            {
                valueAsTask = Task.FromResult(saved.Value.Value.AsImmutable());
                absoluteExpiration = saved.Value.AbsoluteExpiration;
                slidingExpiration = saved.Value.SlidingExpiration;
            }
            else
            {
                valueAsTask = EmptyValueTask;
                absoluteExpiration = null;
                slidingExpiration = null;
            }

            // set the accessed time regardless so we make no further deferred attempts to read from storage
            accessed = clock.UtcNow;
            return valueAsTask.Result;
        }

        /// <summary>
        /// Clear the content from storage and release it from memory.
        /// </summary>
        public Task RemoveAsync()
        {
            // reset the in-memory value
            valueAsTask = EmptyValueTask;

            // also attempt to clear storage
            return storage.ClearAsync(identity.PrimaryKeyString);
        }

        /// <summary>
        /// Sets content and expiration options for a cache key.
        /// </summary>
        public Task SetAsync(Immutable<byte[]> value, DateTimeOffset? absoluteExpiration, TimeSpan? slidingExpiration)
        {
            // we cache the task itself to reduce allocations over time on the GET method
            valueAsTask = Task.FromResult(value);
            this.absoluteExpiration = absoluteExpiration;
            this.slidingExpiration = slidingExpiration;
            accessed = clock.UtcNow;

            // also attempt to save to storage
            return storage.WriteAsync(identity.PrimaryKeyString, value.Value, absoluteExpiration, slidingExpiration);
        }

        /// <summary>
        /// Touches the cached item without retrieving it as to delay the sliding expiration.
        /// </summary>
        public Task RefreshAsync()
        {
            // if the accessed time is set then we have already loaded this content
            // we can therefore shortcut this call to only set the accessed time again
            if (accessed.HasValue)
            {
                accessed = clock.UtcNow;
                return Task.CompletedTask;
            }

            // otherwise we need to load from storage to keep the state consistent
            // this operation will set the accessed time in the process
            return LoadAsync();
        }

        public async Task Invoke(IIncomingGrainCallContext context)
        {
            // we do not yet tolerate any errors on this grain
            // we will refactor this once the grain supports deffered saving
            try
            {
                await context.Invoke().ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                DeactivateOnIdle();
                logger.LogError(ex, ex.Message);
                throw;
            }
        }

        private static readonly Task<Immutable<byte[]>> EmptyValueTask = Task.FromResult(((byte[])null).AsImmutable());
    }
}