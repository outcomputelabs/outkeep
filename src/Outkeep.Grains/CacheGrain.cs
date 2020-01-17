using Orleans;
using Orleans.Concurrency;
using Orleans.Providers;
using Orleans.Runtime;
using Outkeep.Core;
using Outkeep.Core.Caching;
using Outkeep.Interfaces;
using System;
using System.Threading.Tasks;

namespace Outkeep.Grains
{
    [Reentrant]
    [StorageProvider(ProviderName = OutkeepProviderNames.OutkeepCache)]
    internal class CacheGrain : Grain, ICacheGrain
    {
        private readonly ICacheGrainContext _context;
        private readonly IPersistentState<CacheGrainState> _state;
        private readonly IPersistentState<CacheGrainFlags> _flags;

        public CacheGrain(ICacheGrainContext context, [PersistentState("State")] IPersistentState<CacheGrainState> state, IPersistentState<CacheGrainFlags> flags)
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
            // release the space claim early
            _entry?.Expire();

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

                // release it for garbage collection
                _entry = null;

                // also attempt to remove it from storage
                return ClearAllStateAsync();
            }

            // otherwise the entry was evicted due to capacity reasons so shutdown the grain
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
        public ValueTask<CachePulse> GetAsync()
        {
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
            _state.State.Tag = Guid.NewGuid();
            _state.State.Value = null;

            // fulfill any pending promises
            Publish();

            // attempt to clear storage as well
            return ClearAllStateAsync();
        }

        /// <inheritdoc />
        public Task SetAsync(Immutable<byte[]?> value, DateTimeOffset? absoluteExpiration, TimeSpan? slidingExpiration)
        {
            // if we have an allocation then expire it and release it for garbage collection
            if (_entry != null)
            {
                _entry?.Expire();
                _entry = null;
            }

            // check if there is anything to set at all
            if (value.Value is null)
            {
                // storing a null value is equivalent to a clear
                _state.State.Tag = Guid.NewGuid();
                _state.State.Value = null;
                Publish();
                return ClearAllStateAsync();
            }

            // check if the input has already expired
            var now = _context.Clock.UtcNow;
            if (absoluteExpiration.HasValue && absoluteExpiration.Value <= now ||
                (slidingExpiration.HasValue && _flags.State.UtcLastAccessed.Add(slidingExpiration.Value) <= now))
            {
                // the input has already expired so there is no point adding it
                // for our purpose the state has cleared
                _state.State.Tag = Guid.NewGuid();
                _state.State.Value = null;
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
            if (tag == _state.State.Tag)
            {
                return new ValueTask<CachePulse>(_promise.Task.WithDefaultOnTimeout(new CachePulse(_state.State.Tag, null), _context.Options.ReactivePollingTimeout));
            }

            return new ValueTask<CachePulse>(new CachePulse(_state.State.Tag, _state.State.Value));
        }
    }
}