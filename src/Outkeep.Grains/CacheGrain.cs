using Orleans;
using Orleans.Concurrency;
using Orleans.Runtime;
using Outkeep.Core.Caching;
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
                .SetPriority(CachePriority.Normal)
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

            // check if the memory allowance was revoked
            if (self._entry != null && self._entry.IsRevoked)
            {
                // deactivate the grain to allow cluster rebalancing
                self.DeactivateOnIdle();
                self.Publish(false);
                return Task.CompletedTask;
            }

            // check if there is any content to evaluate
            if (self._state.State.Tag == Guid.Empty)
            {
                return Task.CompletedTask;
            }

            // check if the content has expired
            if (self.IsExpired())
            {
                return self.ResetAsync();
            }

            // save the flags if they have changed
            if (self._flags.State.HasChanged)
            {
                self._flags.State.HasChanged = false;
                return self._flags.WriteStateAsync();
            }

            return Task.CompletedTask;
        }

        private Task ResetAsync()
        {
            // release the memory allowance
            ReleaseToken();

            // clear and publish the state
            _state.State.Tag = Guid.Empty;
            _state.State.Value = null;
            _state.State.AbsoluteExpiration = null;
            _state.State.SlidingExpiration = null;
            Publish();

            // attempt to remove stored state as well
            return Task.WhenAll(_state.ClearStateAsync(), _flags.ClearStateAsync());
        }

        private void ReleaseToken()
        {
            if (_entry is null) return;

            _entry.Revoke();
            _entry = null;
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
            DeactivateOnIdle();
            return ResetAsync();
        }

        /// <inheritdoc />
        public Task SetAsync(Immutable<byte[]?> value, DateTimeOffset? absoluteExpiration, TimeSpan? slidingExpiration)
        {
            ReleaseToken();

            // check if there is anything to set at all
            // setting null content is equivalent to clearing
            if (value.Value is null)
            {
                // noop if there is nothing to clear
                if (_state.State.Tag == Guid.Empty) return Task.CompletedTask;

                return ResetAsync();
            }

            // check if the input has already expired
            var now = _context.Clock.UtcNow;
            if (absoluteExpiration.HasValue && absoluteExpiration.Value <= now)
            {
                // noop if there is nothing to clear
                if (_state.State.Tag == Guid.Empty) return Task.CompletedTask;

                return ResetAsync();
            }

            // claim space in the current box
            _flags.State.UtcLastAccessed = now;
            _entry = _context.Director
                .CreateEntry(PrimaryKey, value.Value.Length)
                .SetPriority(CachePriority.Normal)
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