using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Concurrency;
using Outkeep.Core.Caching;
using Outkeep.Core.Storage;
using Outkeep.Grains.Properties;
using Outkeep.Interfaces;
using System;
using System.Threading.Tasks;

namespace Outkeep.Grains
{
    [Reentrant]
    internal class CacheGrain : Grain, ICacheGrain, IIncomingGrainCallFilter
    {
        private readonly ICacheGrainContext _context;

        public CacheGrain(ICacheGrainContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        private CachePulse _pulse = CachePulse.None;
        private TaskCompletionSource<CachePulse> _promise = new TaskCompletionSource<CachePulse>(CachePulse.None);
        private Task? _outstandingStorageOperation = null;
        private ICacheEntry? _entry;

        private string PrimaryKey => this.GetPrimaryKeyString();

        public override async Task OnActivateAsync()
        {
            // attempt to load from storage
            var item = await _context.Storage.ReadAsync(PrimaryKey).ConfigureAwait(true);
            if (!item.HasValue)
            {
                SetPulse(CachePulse.None);
                return;
            }

            // attempt to claim space in the current box
            _entry = _context.Director
                .CreateEntry(PrimaryKey, item.Value.Value.Length + IntPtr.Size)
                .SetAbsoluteExpiration(item.Value.AbsoluteExpiration)
                .SetSlidingExpiration(item.Value.SlidingExpiration)
                .SetPriority(CachePriority.Normal)
                .SetUtcLastAccessed(_context.Clock.UtcNow)
                .ContinueWithOnEvicted(HandleEvicted)
                .Commit();

            // see if we were successful and if not then release the content now
            if (_entry.TryExpire(_context.Clock.UtcNow))
            {
                _entry = null;
                SetPulse(CachePulse.None);
                return;
            }

            // otherwise keep the content
            SetPulse(new CachePulse(Guid.NewGuid(), new Immutable<byte[]?>(item.Value.Value)));
        }

        public override Task OnDeactivateAsync()
        {
            ClearEntry();

            return Task.CompletedTask;
        }

        private void HandleEvicted(Task<CacheEvictionArgs> args)
        {
            if (args.Result.CacheEntry == _entry)
            {
                ClearEntry();
                SetPulse(CachePulse.None);
            }
        }

        private void SetPulse(CachePulse pulse)
        {
            if (pulse == _pulse) return;

            _pulse = pulse;
            _promise.TrySetResult(pulse);
            _promise = new TaskCompletionSource<CachePulse>();
        }

        private void ClearEntry()
        {
            if (_entry != null)
            {
                _entry.Expire();
                _entry = null;
            }
        }

        /// <inheritdoc />
        public ValueTask<CachePulse> GetAsync()
        {
            if (_entry != null)
            {
                _entry.UtcLastAccessed = _context.Clock.UtcNow;
            }

            return new ValueTask<CachePulse>(_pulse);
        }

        /// <inheritdoc />
        public Task RemoveAsync()
        {
            // check if we have any entry active
            if (_entry == null)
            {
                // this grain has no entry so there is nothing to do
                return Task.CompletedTask;
            }

            // unconditionally expire and release the entry for gc
            _entry.Expire();
            _entry = null;

            // fulfill any pending requests
            SetPulse(CachePulse.None);

            // remove the entry from storage
            return PersistAsync();
        }

        /// <inheritdoc />
        public Task SetAsync(Immutable<byte[]?> value, DateTimeOffset? absoluteExpiration, TimeSpan? slidingExpiration)
        {
            // clear any previous entry
            ClearEntry();

            // check if there is anything to set at all
            if (value.Value is null)
            {
                // storing a null value is equivalent to a clear
                SetPulse(CachePulse.None);
                return PersistAsync();
            }

            // claim space in the current box
            _entry = _context.Director
                .CreateEntry(PrimaryKey, value.Value.Length + IntPtr.Size)
                .SetAbsoluteExpiration(absoluteExpiration)
                .SetSlidingExpiration(slidingExpiration)
                .SetPriority(CachePriority.Normal)
                .SetUtcLastAccessed(_context.Clock.UtcNow)
                .ContinueWithOnEvicted(HandleEvicted)
                .Commit();

            // update subs and lazy save
            SetPulse(new CachePulse(Guid.NewGuid(), value));
            return PersistAsync();
        }

        /// <summary>
        /// Executes storage operations in a reentrancy-safe way.
        /// </summary>
        private async Task PersistAsync()
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
                    throw new InvalidOperationException(Resources.Exception_CacheGrainForKey_X_CannotExecuteStorageOperationBecauseThePriorOperationFailed.Format(PrimaryKey), ex);
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

                if (_entry is null || _pulse.Value.Value == null)
                {
                    _outstandingStorageOperation = _context.Storage.ClearAsync(PrimaryKey);
                }
                else
                {
                    _outstandingStorageOperation = _context.Storage.WriteAsync(PrimaryKey, new CacheItem(_pulse.Value.Value, _entry.AbsoluteExpiration, _entry.SlidingExpiration));
                }

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
            if (_entry != null)
            {
                _entry.UtcLastAccessed = _context.Clock.UtcNow;
            }

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task<CachePulse> PollAsync(Guid tag)
        {
            // if the user tag is different than the current tag then return the last pulse
            if (tag != _pulse.Tag)
            {
                return Task.FromResult(_pulse);
            }

            // otherwise if there is no entry yet then return a promise until we have one
            if (_entry == null)
            {
                return _promise.Task.WithDefaultOnTimeout(_pulse, _context.Options.ReactivePollingTimeout);
            }

            // if the current entry has expired then release it and return a clear pulse
            var now = _context.Clock.UtcNow;
            if (_entry.TryExpire(now))
            {
                _entry = null;
                SetPulse(CachePulse.None);
                return Task.FromResult(CachePulse.None);
            }

            // if the entry has not expired then decide based on the input tag
            _entry.UtcLastAccessed = now;
            if (tag == _pulse.Tag)
            {
                // the caller already has the same data so return a promise
                return _promise.Task.WithDefaultOnTimeout(CachePulse.None, _context.Options.ReactivePollingTimeout);
            }
            else
            {
                // the caller has a different version of data so return the current one now
                return Task.FromResult(_pulse);
            }
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
                Log.Failed(_context.Logger, PrimaryKey, ex);
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
    }
}