using Microsoft.Extensions.Options;
using Orleans;
using Orleans.Concurrency;
using Orleans.Runtime;
using Outkeep.Governance;
using System;
using System.Threading.Tasks;

namespace Outkeep.Caching
{
    [Reentrant]
    internal class CacheGrain : Grain, ICacheGrain
    {
        private readonly CacheOptions _options;
        private readonly ISystemClock _clock;
        private readonly IPersistentState<CacheGrainState> _state;
        private readonly IPersistentState<CacheGrainFlags> _flags;
        private readonly IWeakActivationState<ActivityState> _activity;

        public CacheGrain(
            IOptions<CacheOptions> options,
            ISystemClock clock,
            [PersistentState("State", OutkeepProviderNames.OutkeepCache)] IPersistentState<CacheGrainState> state,
            [PersistentState("Flags", OutkeepProviderNames.OutkeepCache)] IPersistentState<CacheGrainFlags> flags,
            [WeakActivationState(OutkeepProviderNames.OutkeepMemoryResourceGovernor)] IWeakActivationState<ActivityState> activity)
        {
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _state = state?.AsConflater() ?? throw new ArgumentNullException(nameof(state));
            _flags = flags?.AsConflater() ?? throw new ArgumentNullException(nameof(flags));
            _activity = activity ?? throw new ArgumentNullException(nameof(activity));
        }

        private TaskCompletionSource<CachePulse> _promise = new TaskCompletionSource<CachePulse>();

        public override Task OnActivateAsync()
        {
            RegisterTimer(TickMaintenanceAsyncDelegate, this, _options.MaintenancePeriod, _options.MaintenancePeriod);

            // remove expired state
            var recoveryTask =
                _state.State.Value != null && IsExpired()
                ? ResetAsync()
                : Task.CompletedTask;

            // enroll as a weak activation
            _activity.State.Priority = ActivityPriority.Normal;
            var enlistTask = _activity.EnlistAsync();

            return Task.WhenAll(recoveryTask, enlistTask);
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
            // clear and publish the state
            _state.State.Tag = Guid.Empty;
            _state.State.Value = null;
            _state.State.AbsoluteExpiration = null;
            _state.State.SlidingExpiration = null;
            Publish();

            // attempt to remove stored state as well
            return Task.WhenAll(_state.ClearStateAsync(), _flags.ClearStateAsync());
        }

        private bool IsExpired()
        {
            var now = _clock.UtcNow;
            return
                _state.State.AbsoluteExpiration.HasValue && _state.State.AbsoluteExpiration <= now ||
                _state.State.SlidingExpiration.HasValue && _flags.State.UtcLastAccessed.Add(_state.State.SlidingExpiration.Value) <= now;
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
            _flags.State.UtcLastAccessed = _clock.UtcNow;

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
            // check if there is anything to set at all
            // setting null content is equivalent to clearing
            if (value.Value is null)
            {
                // noop if there is nothing to clear
                if (_state.State.Tag == Guid.Empty) return Task.CompletedTask;

                return ResetAsync();
            }

            // check if the input has already expired
            var now = _clock.UtcNow;
            if (absoluteExpiration.HasValue && absoluteExpiration.Value <= now)
            {
                // noop if there is nothing to clear
                if (_state.State.Tag == Guid.Empty) return Task.CompletedTask;

                return ResetAsync();
            }

            _flags.State.UtcLastAccessed = now;

            // update subs and lazy save everything
            _state.State.Tag = Guid.NewGuid();
            _state.State.Value = value.Value;
            _state.State.AbsoluteExpiration = absoluteExpiration;
            _state.State.SlidingExpiration = slidingExpiration;
            Publish();
            return Task.WhenAll(_state.WriteStateAsync(), _flags.WriteStateAsync());
        }

        /// <inheritdoc />
        public Task RefreshAsync()
        {
            _flags.State.UtcLastAccessed = _clock.UtcNow;

            return _flags.WriteStateAsync();
        }

        /// <inheritdoc />
        public ValueTask<CachePulse> PollAsync(Guid tag)
        {
            _flags.State.UtcLastAccessed = _clock.UtcNow;

            if (tag == _state.State.Tag)
            {
                return new ValueTask<CachePulse>(_promise.Task.WithDefaultOnTimeout(new CachePulse(_state.State.Tag, null), _options.ReactivePollingTimeout));
            }

            return new ValueTask<CachePulse>(new CachePulse(_state.State.Tag, _state.State.Value));
        }
    }
}