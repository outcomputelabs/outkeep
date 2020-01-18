using Orleans;
using Orleans.Concurrency;
using Orleans.Runtime;
using Outkeep.Core.Caching;
using Outkeep.Interfaces;
using System;
using System.Threading.Tasks;

namespace Outkeep.Grains
{
    [Reentrant]
    internal class CacheGrain : Grain, ICacheGrain
    {
        private readonly ICacheGrainContext _context;
        private readonly IPersistentState<CacheGrainState> _state;
        private readonly IPersistentState<CacheGrainFlags> _flags;

        public CacheGrain(ICacheGrainContext context, [PersistentState("State", OutkeepProviderNames.OutkeepCache)] IPersistentState<CacheGrainState> state, [PersistentState("Flags", OutkeepProviderNames.OutkeepCache)] IPersistentState<CacheGrainFlags> flags)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _state = state?.AsConflater() ?? throw new ArgumentNullException(nameof(state));
            _flags = flags?.AsConflater() ?? throw new ArgumentNullException(nameof(flags));
        }

        private TaskCompletionSource<CachePulse> _promise = new TaskCompletionSource<CachePulse>();
        private ICacheEntry<string>? _entry;

        private string PrimaryKey => this.GetPrimaryKeyString();

        public override Task OnActivateAsync()
        {
            RegisterTimer(TickMaintenanceAsyncDelegate, this, _context.Options.MaintenancePeriod, _context.Options.MaintenancePeriod);

            // see if we have state to recover
            if (_state.State.Value is null)
            {
                // there is nothing to recover
                return Task.CompletedTask;
            }

            // see if the stored state has expired
            if (IsExpired())
            {
                // the state has expired so remove it early
                return ClearAllStateAsync();
            }

            // attempt to claim space in the current box for the loaded state
            _flags.State.UtcLastAccessed = _context.Clock.UtcNow;
            _entry = _context.Director
                .CreateEntry(PrimaryKey, _state.State.Value.Length)
                .SetAbsoluteExpiration(_state.State.AbsoluteExpiration)
                .SetSlidingExpiration(_state.State.SlidingExpiration)
                .SetPriority(CachePriority.Normal)
                .SetUtcLastAccessed(_flags.State.UtcLastAccessed)
                .ContinueWithOnEvicted(HandleEvictedAsync)
                .Commit();

            return Task.CompletedTask;
        }

        public override Task OnDeactivateAsync()
        {
            // release the space claim early but without expiring
            _entry?.Dispose();

            return Task.CompletedTask;
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
            var myself = (CacheGrain)state;

            if (myself._flags.State.HasChanged)
            {
                myself._flags.State.HasChanged = false;
                return myself._flags.WriteStateAsync();
            }

            return Task.CompletedTask;
        }

        private Task ClearAllStateAsync()
        {
            return Task.WhenAll(_state.ClearStateAsync(), _flags.ClearStateAsync());
        }

        private Task WriteAllStateAsync()
        {
            return Task.WhenAll(_state.WriteStateAsync(), _flags.WriteStateAsync());
        }

        private bool IsExpired()
        {
            var now = _context.Clock.UtcNow;
            return
                (_state.State.AbsoluteExpiration.GetValueOrDefault(DateTimeOffset.MaxValue) <= now) ||
                (_state.State.SlidingExpiration.HasValue && _flags.State.UtcLastAccessed.Add(_state.State.SlidingExpiration.Value) <= now);
        }

        private Task HandleEvictedAsync(CacheEvictionArgs<string> args)
        {
            // see if we are receiving this out-of-band
            if (args.CacheEntry != _entry) return Task.CompletedTask;

            // see why the entry was evicted
            if (_entry.EvictionCause == EvictionCause.Expired)
            {
                // if the entry expired then propagate it asap to users
                _state.State.Tag = Guid.Empty;
                _state.State.Value = null;
                Publish();

                // release the entry for garbage collection
                _entry = null;

                // attempt to remove the content from storage
                return ClearAllStateAsync();
            }

            // otherwise the entry was evicted due to capacity reasons
            // therefore shutdown the grain to release the content for garbage collection
            DeactivateOnIdle();

            // early resolve any pending promises so the grain can shutdown
            Publish(false);

            return Task.CompletedTask;
        }

        private void Publish(bool success = true)
        {
            if (success)
            {
                _promise.TrySetResult(new CachePulse(_state.State.Tag, _state.State.Value));
            }
            else
            {
                _promise.TrySetCanceled();
            }

            _promise = new TaskCompletionSource<CachePulse>();
        }

        /// <inheritdoc />
        [ReadOnly]
        public ValueTask<CachePulse> GetAsync()
        {
            _flags.State.UtcLastAccessed = _context.Clock.UtcNow;

            if (_entry != null)
            {
                _entry.UtcLastAccessed = _flags.State.UtcLastAccessed;
            }

            return new ValueTask<CachePulse>(new CachePulse(_state.State.Tag, _state.State.Value));
        }

        /// <inheritdoc />
        public Task RemoveAsync()
        {
            // if we have an allocation then expire it and release it for garbage collection
            if (_entry != null)
            {
                _entry?.Expire();
                _entry = null;
            }

            // reset the state to fulfill get requests
            _state.State.Tag = Guid.Empty;
            _state.State.Value = null;
            _state.State.AbsoluteExpiration = null;
            _state.State.SlidingExpiration = null;

            // fulfill any pending promises
            Publish();

            // attempt to clear storage as well
            return ClearAllStateAsync();
        }

        /// <inheritdoc />
        public Task SetAsync(Immutable<byte[]?> value, DateTimeOffset? absoluteExpiration, TimeSpan? slidingExpiration)
        {
            // if we have a previous allocation then expire it and release it for garbage collection
            if (_entry != null)
            {
                _entry?.Expire();
                _entry = null;
            }

            // check if there is anything to set at all
            if (value.Value is null)
            {
                // storing a null value is equivalent to a clear
                _state.State.Tag = Guid.Empty;
                _state.State.Value = null;
                _state.State.AbsoluteExpiration = null;
                _state.State.SlidingExpiration = null;
                Publish();
                return ClearAllStateAsync();
            }

            // check if the input has already expired
            var now = _context.Clock.UtcNow;
            if (absoluteExpiration.HasValue && absoluteExpiration.Value <= now)
            {
                // the input has already expired so there is no point adding it
                // for our purpose the state has cleared
                _state.State.Tag = Guid.Empty;
                _state.State.Value = null;
                _state.State.AbsoluteExpiration = null;
                _state.State.SlidingExpiration = null;
                Publish();
                return ClearAllStateAsync();
            }

            // claim space in the current box
            _flags.State.UtcLastAccessed = now;
            _entry = _context.Director
                .CreateEntry(PrimaryKey, value.Value.Length)
                .SetAbsoluteExpiration(absoluteExpiration)
                .SetSlidingExpiration(slidingExpiration)
                .SetPriority(CachePriority.Normal)
                .SetUtcLastAccessed(_flags.State.UtcLastAccessed)
                .ContinueWithOnEvicted(HandleEvictedAsync)
                .Commit();

            // update subs and lazy save everything
            _state.State.Tag = Guid.NewGuid();
            _state.State.Value = value.Value;
            _state.State.AbsoluteExpiration = absoluteExpiration;
            _state.State.SlidingExpiration = slidingExpiration;
            Publish();
            return WriteAllStateAsync();
        }

        /// <inheritdoc />
        public Task RefreshAsync()
        {
            _flags.State.UtcLastAccessed = _context.Clock.UtcNow;

            if (_entry != null)
            {
                _entry.UtcLastAccessed = _flags.State.UtcLastAccessed;
            }

            return _flags.WriteStateAsync();
        }

        /// <inheritdoc />
        public ValueTask<CachePulse> PollAsync(Guid tag)
        {
            _flags.State.UtcLastAccessed = _context.Clock.UtcNow;

            if (_entry != null)
            {
                _entry.UtcLastAccessed = _flags.State.UtcLastAccessed;
            }

            if (tag == _state.State.Tag)
            {
                return new ValueTask<CachePulse>(_promise.Task.WithDefaultOnTimeout(new CachePulse(_state.State.Tag, null), _context.Options.ReactivePollingTimeout));
            }

            return new ValueTask<CachePulse>(new CachePulse(_state.State.Tag, _state.State.Value));
        }
    }
}