using Orleans;
using Orleans.Concurrency;
using Orleans.Runtime;
using Outkeep.Governance;
using System;
using System.Threading.Tasks;

namespace Outkeep.Caching
{
    internal class CacheGrain : Grain, ICacheGrain
    {
        private readonly ICacheActivationContext _context;
        private readonly IPersistentState<CacheGrainState> _state;
        private readonly IPersistentState<CacheGrainFlags> _flags;
        private readonly IWeakActivationState<ActivityState> _activity;

        public CacheGrain(
            ICacheActivationContext context,
            [PersistentState("State", OutkeepProviderNames.OutkeepCache)] IPersistentState<CacheGrainState> state,
            [PersistentState("Flags", OutkeepProviderNames.OutkeepCache)] IPersistentState<CacheGrainFlags> flags,
            [WeakActivationState(OutkeepProviderNames.OutkeepMemoryResourceGovernor)] IWeakActivationState<ActivityState> activity)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _state = state?.AsConflater() ?? throw new ArgumentNullException(nameof(state));
            _flags = flags?.AsConflater() ?? throw new ArgumentNullException(nameof(flags));
            _activity = activity ?? throw new ArgumentNullException(nameof(activity));
        }

        private TaskCompletionSource<CachePulse> _promise = new TaskCompletionSource<CachePulse>();

        private string GrainKey => this.GetPrimaryKeyString();
        private CacheRegistryEntry? _entry = null;

        public override async Task OnActivateAsync()
        {
            // check if the recovered state has expired between activations
            if (IsExpired())
            {
                await RemoveAsync().ConfigureAwait(true);
            }

            // get the stored registry entry for etag reuse
            _entry = await _context.CacheRegistry.GetAsync(GrainKey).ConfigureAwait(true);

            // ensure the registry has up-to-date info in case a prior operation failed
            _entry = await _context.CacheRegistry
                .RegisterOrUpdateAsync(new CacheRegistryEntry(GrainKey, _state.State.Value?.Length, _state.State.AbsoluteExpiration, _state.State.SlidingExpiration, _entry?.ETag))
                .ConfigureAwait(true);

            // enroll as a weak activation
            _activity.State.Priority = ActivityPriority.Normal;
            await _activity.EnlistAsync().ConfigureAwait(true);

            // start the maintenance schedule
            RegisterTimer(TickMaintenanceAsyncDelegate, this, _context.Options.MaintenancePeriod, _context.Options.MaintenancePeriod);
        }

        /// <summary>
        /// Caches the maintenance delegate to avoid redundant allocations.
        /// </summary>
        private static readonly Func<object, Task> TickMaintenanceAsyncDelegate = TickMaintenanceAsync;

        /// <summary>
        /// Performs maintenance tasks on the grain.
        /// </summary>
        private static Task TickMaintenanceAsync(object state)
        {
            var self = (CacheGrain)state;

            // check if the content has expired
            if (self.IsExpired())
            {
                return self.RemoveAsync();
            }

            // save control flags if changed
            if (self._flags.State.UtcLastAccessed != self._flags.State.PersistedUtcLastAccessed)
            {
                self._flags.State.PersistedUtcLastAccessed = self._flags.State.UtcLastAccessed;
                return self._flags.WriteStateAsync();
            }

            return Task.CompletedTask;
        }

        private bool IsExpired()
        {
            var now = _context.Clock.UtcNow;
            return
                _state.State.AbsoluteExpiration.HasValue && _state.State.AbsoluteExpiration <= now ||
                _state.State.SlidingExpiration.HasValue && _flags.State.UtcLastAccessed.Add(_state.State.SlidingExpiration.Value) <= now;
        }

        /// <summary>
        /// Publishes the current cache content to any pending reactive requests.
        /// </summary>
        private void Publish()
        {
            _promise.TrySetResult(new CachePulse(_state.State.Tag, _state.State.Value));
            _promise = new TaskCompletionSource<CachePulse>();
        }

        /// <inheritdoc />
        public Task RemoveAsync()
        {
            // removing the cache item is equivalent to setting it to null and vice-versa
            return SetAsync(new Immutable<byte[]?>(), null, null);
        }

        /// <inheritdoc />
        public async Task SetAsync(Immutable<byte[]?> value, DateTimeOffset? absoluteExpiration, TimeSpan? slidingExpiration)
        {
            // check if there is anything to set at all - setting null content is equivalent to clearing
            // also check if the input has already expired
            // if either then we clear the current state
            var now = _context.Clock.UtcNow;
            if (value.Value is null || (absoluteExpiration.HasValue && absoluteExpiration.Value <= now))
            {
                // clear the state
                _state.State.Value = default;
                _state.State.Tag = default;
                _state.State.AbsoluteExpiration = default;
                _state.State.SlidingExpiration = default;

                // propagate the change to reactive requesters
                Publish();

                // attempt to clear the state from storage
                await Task.WhenAll(_state.ClearStateAsync(), _flags.ClearStateAsync()).ConfigureAwait(true);

                // unregister from the cache registry last
                _context.CacheRegistry.UnregisterAsync(_entry ?? new CacheRegistryEntry(GrainKey, null, null, null))
                await _context.CacheRegistry.UnregisterAsync(GrainKey).ConfigureAwait(true);
            }
            else
            {
                // register on cache registry first
                await _context.CacheRegistry.RegisterAsync(GrainKey, value.Value.Length).ConfigureAwait(true);

                // now we can accept the new data
                _state.State.Tag = Guid.NewGuid();
                _state.State.Value = value.Value;
                _state.State.AbsoluteExpiration = absoluteExpiration;
                _state.State.SlidingExpiration = slidingExpiration;

                // start measuring sliding expiration from this point onwards
                _flags.State.UtcLastAccessed = now;

                // propagate the change to reactive requesters
                Publish();

                // attempt to write state to storage while saving control flags
                await Task.WhenAll(_state.WriteStateAsync(), _flags.WriteStateAsync()).ConfigureAwait(true);
            }
        }

        /// <inheritdoc />
        public ValueTask<CachePulse> GetAsync()
        {
            // delay the sliding expiration
            _flags.State.UtcLastAccessed = _context.Clock.UtcNow;

            return new ValueTask<CachePulse>(new CachePulse(_state.State.Tag, _state.State.Value));
        }

        /// <inheritdoc />
        public ValueTask<CachePulse> PollAsync(Guid tag)
        {
            // delay the sliding expiration
            _flags.State.UtcLastAccessed = _context.Clock.UtcNow;

            // if the caller already has the latest state then return a promise for the next state
            if (tag == _state.State.Tag)
            {
                return new ValueTask<CachePulse>(_promise.Task.WithDefaultOnTimeout(new CachePulse(_state.State.Tag, null), _context.Options.ReactivePollingTimeout));
            }

            // if the caller is outdated then return the latest state right now
            return new ValueTask<CachePulse>(new CachePulse(_state.State.Tag, _state.State.Value));
        }

        /// <inheritdoc />
        public Task RefreshAsync()
        {
            // delay the sliding expiration
            _flags.State.UtcLastAccessed = _context.Clock.UtcNow;

            return Task.CompletedTask;
        }
    }
}