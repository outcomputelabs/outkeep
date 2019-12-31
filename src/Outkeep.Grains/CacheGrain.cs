using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans;
using Orleans.Concurrency;
using Orleans.Core;
using Orleans.Timers;
using Outkeep.Core;
using Outkeep.Grains.Properties;
using Outkeep.Interfaces;
using System;
using System.Threading.Tasks;

namespace Outkeep.Grains
{
    [Reentrant]
    internal class CacheGrain : Grain, ICacheGrain, IIncomingGrainCallFilter
    {
        private static readonly ReactiveResult<Guid, byte[]?> EmptyResult = new ReactiveResult<Guid, byte[]?>(Guid.Empty, null);

        private readonly CacheGrainOptions _options;
        private readonly ILogger<CacheGrain> _logger;
        private readonly ITimerRegistry _timerRegistry;
        private readonly ICacheStorage _storage;
        private readonly ISystemClock _clock;
        private readonly IGrainIdentity _identity;

        public CacheGrain(IOptions<CacheGrainOptions> options, ILogger<CacheGrain> logger, ITimerRegistry timerRegistry, ICacheStorage storage, ISystemClock clock, IGrainIdentity identity)
        {
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _timerRegistry = timerRegistry ?? throw new ArgumentNullException(nameof(timerRegistry));
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _identity = identity ?? throw new ArgumentNullException(nameof(identity));
        }

        private ReactiveResult<Guid, byte[]?> _result = new ReactiveResult<Guid, byte[]?>(Guid.NewGuid(), null);
        private DateTimeOffset? _absoluteExpiration;
        private TimeSpan? _slidingExpiration;
        private DateTimeOffset _accessed;

        private Task? _outstandingStorageOperation = null;

        private TaskCompletionSource<ReactiveResult<Guid, byte[]?>> _reactiveTask = new TaskCompletionSource<ReactiveResult<Guid, byte[]?>>();

        public override async Task OnActivateAsync()
        {
            // start an expiry policy evaluation timer
            _timerRegistry.RegisterTimer(this, TickExpirationPolicy, null, _options.ExpirationPolicyEvaluationPeriod, _options.ExpirationPolicyEvaluationPeriod);

            // attempt to load from storage
            var item = await _storage.ReadAsync(_identity.PrimaryKeyString).ConfigureAwait(true);
            if (item.HasValue)
            {
                _result = new ReactiveResult<Guid, byte[]?>(Guid.NewGuid(), item.Value.Value);
                _absoluteExpiration = item.Value.AbsoluteExpiration;
                _slidingExpiration = item.Value.SlidingExpiration;
            }
            _accessed = _clock.UtcNow;
        }

        private Task TickExpirationPolicy(object state)
        {
            // quick path for nothing to remove
            if (_result.Value == null) return Task.CompletedTask;

            // quick-ish path for item not yet expired
            if (!IsExpired()) return Task.CompletedTask;

            // slow path for removing the item
            return RemoveAsync();
        }

        private void SetReactivePromise()
        {
            // fulfill the pending reactive promise
            _reactiveTask.SetResult(_result);

            // create a new pending reactive promise
            _reactiveTask = new TaskCompletionSource<ReactiveResult<Guid, byte[]?>>();
        }

        /// <summary>
        /// Returns <see cref="true"/> if the item has expired, otherwise <see cref="false"/>.
        /// </summary>
        private bool IsExpired()
        {
            var now = _clock.UtcNow;

            return
                (_absoluteExpiration.HasValue && _absoluteExpiration.Value <= now) ||
                (_slidingExpiration.HasValue && _accessed.Add(_slidingExpiration.Value) <= now);
        }

        /// <inheritdoc />
        public ValueTask<Immutable<byte[]?>> GetAsync()
        {
            _accessed = _clock.UtcNow;

            return new ValueTask<Immutable<byte[]?>>(_result.Value.AsImmutable());
        }

        /// <inheritdoc />
        public Task RemoveAsync()
        {
            _result = new ReactiveResult<Guid, byte[]?>(Guid.NewGuid(), null);
            _absoluteExpiration = null;
            _slidingExpiration = null;
            _accessed = _clock.UtcNow;

            SetReactivePromise();

            return StorageOperationAsync(StorageOperationType.Clear);
        }

        /// <inheritdoc />
        public Task SetAsync(Immutable<byte[]?> value, DateTimeOffset? absoluteExpiration, TimeSpan? slidingExpiration)
        {
            _result = new ReactiveResult<Guid, byte[]?>(Guid.NewGuid(), value.Value);
            _absoluteExpiration = absoluteExpiration;
            _slidingExpiration = slidingExpiration;
            _accessed = _clock.UtcNow;

            SetReactivePromise();

            return StorageOperationAsync(StorageOperationType.Write);
        }

        /// <summary>
        /// Executes storage operations in a reentrancy-safe way.
        /// </summary>
        private async Task StorageOperationAsync(StorageOperationType type)
        {
            // check if there is an outstanding storage operation
            // if there is one it will not include the values we have just staged
            var thisTask = _outstandingStorageOperation;
            if (thisTask != null)
            {
                try
                {
                    await thisTask.ConfigureAwait(true);
                }
                catch (Exception ex)
                {
                    // while we do not want to observe past exceptions
                    // we do want to stop execution early if the outstanding operation failed
                    throw new InvalidOperationException(Resources.Exception_CacheGrainForKey_X_CannotExecuteStorageOperationBecauseThePriorOperationFailed.Format(_identity.PrimaryKeyString), ex);
                }
                finally
                {
                    // if this continuation beat other write attempts then we null out the task to allow further attempts
                    // if the tasks are different then other attempts have already started
                    if (thisTask == _outstandingStorageOperation)
                    {
                        _outstandingStorageOperation = null;
                    }
                }
            }

            // at this point this continuation is either the first to get here or other continuations may have gotten here already
            if (_outstandingStorageOperation == null)
            {
                // this means this is (probably) the first continuation to get to this point
                // therefore this continuation gets to start the next storage operation
                _outstandingStorageOperation = type switch
                {
                    StorageOperationType.Write => _storage.WriteAsync(_identity.PrimaryKeyString, new CacheItem(_result.Value, _absoluteExpiration, _slidingExpiration)),
                    StorageOperationType.Clear => _storage.ClearAsync(_identity.PrimaryKeyString),
                    _ => throw new NotSupportedException(),
                };
                thisTask = _outstandingStorageOperation;
            }
            else
            {
                // getting here means another continuation has already started a write operation
                // that write operation already covers the value of the current task so we can await on it
                thisTask = _outstandingStorageOperation;
            }

            // either way we can now wait for the outstanding operation
            try
            {
                await thisTask.ConfigureAwait(true);
            }
            finally
            {
                // if this continuation beat other write attempts here then we null out the task to allow further attempts
                // otherwise some other write attempt has already started
                if (thisTask == _outstandingStorageOperation)
                {
                    _outstandingStorageOperation = null;
                }
            }
        }

        /// <inheritdoc />
        public Task RefreshAsync()
        {
            _accessed = _clock.UtcNow;
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task<ReactiveResult<Guid, byte[]?>> PollAsync(Guid etag)
        {
            // if the tags are the same then return the reactive promise
            if (etag == _result.ETag)
            {
                return _reactiveTask.Task.WithDefaultOnTimeout(EmptyResult, _options.ReactivePollGracefulTimeout);
            }

            // if the tags are different then return the current value
            return Task.FromResult(_result);
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
                Log.Failed(_logger, _identity.PrimaryKeyString, ex);
                throw;
            }
        }

        private static class Log
        {
            private static readonly Action<ILogger, string, Exception> _failed =
                LoggerMessage.Define<string>(LogLevel.Error, new EventId(1, nameof(Failed)), Resources.Log_CacheGrainForKey_X_FailedAndWillBeDeactivatedNow);

            public static void Failed(ILogger logger, string key, Exception exception) =>
                _failed(logger, key, exception);
        }

        private enum StorageOperationType
        {
            Write,
            Clear
        }
    }
}