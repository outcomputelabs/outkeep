using System;
using System.Threading;
using System.Threading.Tasks;

namespace Outkeep.Core.Caching
{
    internal sealed class OvercapacityCallbackRegistration : IDisposable
    {
        private Action<object?>? _callback;
        private readonly object? _state;
        private readonly TaskScheduler _scheduler;
        private readonly ICacheContext _context;
        private bool _disposed;

        public OvercapacityCallbackRegistration(Action<object?> callback, object? state, TaskScheduler scheduler, ICacheContext context)
        {
            _callback = callback ?? throw new ArgumentNullException(nameof(callback));
            _state = state;
            _scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

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

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _callback, null) != null)
            {
                _context.OnOvercapacityCallbackRegistrationDisposed(this);
            }

            GC.SuppressFinalize(this);
        }

        ~OvercapacityCallbackRegistration()
        {
            Dispose();
        }
    }
}