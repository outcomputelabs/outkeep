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
                .ContinueWithOnEvicted(HandleEvictedAsync)
                .Commit();

            return Task.CompletedTask;
        }

        public override Task OnDeactivateAsync()
        {
            // release the token claim early without expiring
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
            var self = (CacheGrain)state;

            // check if there is any state
            if (self._state.State.Tag == Guid.Empty)
            {
                // nothing to do
                return Task.CompletedTask;
            }

            // check if the content has expired
            if (self.IsExpired())
            {
                // release the token claim
                if (self._entry != null)
                {
                    self._entry.Expire();
                    self._entry = null;
                }

                // clear and publish the state
                self._state.State.Tag = Guid.Empty;
                self._state.State.Value = null;
                self._state.State.AbsoluteExpiration = null;
                self._state.State.SlidingExpiration = null;
                self.Publish();

                // attempt to remove stored state as well
                return self.ClearAllStateAsync();
            }

            // save the flags if they have changed
            if (self._flags.State.HasChanged)
            {
                self._flags.State.HasChanged = false;
                return self._flags.WriteStateAsync();
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
                (_state.State.AbsoluteExpiration.HasValue && _state.State.AbsoluteExpiration <= now) ||
                (_state.State.SlidingExpiration.HasValue && _flags.State.UtcLastAccessed.Add(_state.State.SlidingExpiration.Value) <= now);
        }

        private Task HandleEvictedAsync(CacheEvictionArgs<string> args)
        {
            // see if we are receiving this out-of-band
            if (_entry == null || args.CacheEntry != _entry) return Task.CompletedTask;

            // see if the clawback is due to expiry
            if (_entry.EvictionCause == EvictionCause.Expired)
            {
                // release the entry for garbage collection
                _entry = null;

                // propagate the expiration to pending requests
                _state.State.Tag = Guid.Empty;
                _state.State.Value = null;
                _state.State.AbsoluteExpiration = null;
                _state.State.SlidingExpiration = null;
                Publish();

                // attempt to remove the content from storage
                return ClearAllStateAsync();
            }

            // otherwise the entry was evicted due to capacity reasons
            // therefore shutdown the grain to release the content for garbage collection
            DeactivateOnIdle();

            // early resolve any pending promises to allow the grain to shutdown
            Publish(false);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Publishes the current cache content to any pending reactive requests.
        /// </summary>
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
        public ValueTask<CachePulse> GetAsync()
        {
            _flags.State.UtcLastAccessed = _context.Clock.UtcNow;

            return new ValueTask<CachePulse>(new CachePulse(_state.State.Tag, _state.State.Value));
        }

        /// <inheritdoc />
        public Task RemoveAsync()
        {
            // check if there is anything to remove
            if (_state.State.Tag == Guid.Empty) return Task.CompletedTask;

            // if we have an allocation then expire it and release it for garbage collection
            if (_entry != null)
            {
                _entry.Expire();
                _entry = null;
            }

            // reset the state to fulfill get requests
            _state.State.Tag = Guid.Empty;
            _state.State.Value = null;
            _state.State.AbsoluteExpiration = null;
            _state.State.SlidingExpiration = null;
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
                _entry.Expire();
                _entry = null;
            }

            // check if there is anything to set at all
            // setting null content is equivalent to clearing
            if (value.Value is null)
            {
                // noop if there is nothing to clear
                if (_state.State.Tag == Guid.Empty) return Task.CompletedTask;

                // otherwise clear and propagate
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
                // noop if there is nothing to clear
                if (_state.State.Tag == Guid.Empty) return Task.CompletedTask;

                // otherwise clear and propagate
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

            return _flags.WriteStateAsync();
        }

        /// <inheritdoc />
        public ValueTask<CachePulse> PollAsync(Guid tag)
        {
            _flags.State.UtcLastAccessed = _context.Clock.UtcNow;

            if (tag == _state.State.Tag)
            {
                return new ValueTask<CachePulse>(_promise.Task.WithDefaultOnTimeout(new CachePulse(_state.State.Tag, null), _context.Options.ReactivePollingTimeout));
            }

            return new ValueTask<CachePulse>(new CachePulse(_state.State.Tag, _state.State.Value));
        }
    }
}