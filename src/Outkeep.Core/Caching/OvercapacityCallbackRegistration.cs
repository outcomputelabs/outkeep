using System;
using System.Threading;
using System.Threading.Tasks;

namespace Outkeep.Core.Caching
{
    /// <summary>
    /// Holds a callback registration for overcapacity notifications.
    /// Implements the Disposable Action pattern to notify the appropriate context of disposal.
    /// This class is thread-safe.
    /// </summary>
    internal sealed class OvercapacityCallbackRegistration : IDisposable
    {
        /// <summary>
        /// The callback to invoke.
        /// </summary>
        private Action<object?>? _callback;

        /// <summary>
        /// The state to pass onto the callback.
        /// </summary>
        private readonly object? _state;

        /// <summary>
        /// The task scheduler to schedule the callback on.
        /// This allows callbacks to execute on grain activation task schedulers.
        /// </summary>
        private readonly TaskScheduler _scheduler;

        /// <summary>
        /// The cache context to notify upon disposal of this registration.
        /// </summary>
        private readonly ICacheContext _context;

        /// <summary>
        /// Creates a new <see cref="OvercapacityCallbackRegistration"/> instance.
        /// </summary>
        public OvercapacityCallbackRegistration(Action<object?> callback, object? state, TaskScheduler scheduler, ICacheContext context)
        {
            _callback = callback ?? throw new ArgumentNullException(nameof(callback));
            _state = state;
            _scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Schedules the callback on the given task scheduler.
        /// </summary>
        public void Schedule()
        {
            // capture for thread-safety
            var callback = _callback;

            // no-op if disabled
            if (callback == null) return;

            // schedule the callback
            Task.Factory.StartNew(
                callback,
                _state,
                CancellationToken.None,
                TaskCreationOptions.DenyChildAttach,
                _scheduler);
        }

        #region Disposable

        private bool _disposed;

        public void Dispose()
        {
            if (_disposed) return;

            if (Interlocked.Exchange(ref _callback, null) != null)
            {
                _context.OnOvercapacityCallbackRegistrationDisposed(this);
            }

            GC.SuppressFinalize(this);

            _disposed = true;
        }

        ~OvercapacityCallbackRegistration()
        {
            Dispose();
        }

        #endregion Disposable
    }
}