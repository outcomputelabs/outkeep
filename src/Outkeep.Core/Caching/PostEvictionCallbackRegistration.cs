using System;
using System.Threading;
using System.Threading.Tasks;

namespace Outkeep.Core.Caching
{
    internal sealed class PostEvictionCallbackRegistration : IDisposable
    {
        private readonly Action<object?> _callback;
        private readonly object? _state;
        private readonly TaskScheduler _taskScheduler;
        private readonly ICacheEntryContext _context;

        public PostEvictionCallbackRegistration(Action<object?> callback, object? state, TaskScheduler taskScheduler, ICacheEntryContext context)
        {
            _callback = callback ?? throw new ArgumentNullException(nameof(callback));
            _state = state;
            _taskScheduler = taskScheduler ?? throw new ArgumentNullException(nameof(taskScheduler));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Schedules the callback on the task scheduler if it has not been scheduled yet.
        /// </summary>
        public Task InvokeAsync()
        {
            return Task.Factory.StartNew(_callback, _state, CancellationToken.None, TaskCreationOptions.DenyChildAttach, _taskScheduler);
        }

        #region IDisposable

        private bool disposed;

        public void Dispose()
        {
            if (disposed) return;

            _context.OnPostEvictionCallbackRegistrationDisposed(this);
            GC.SuppressFinalize(this);

            disposed = true;
        }

        ~PostEvictionCallbackRegistration()
        {
            Dispose();
        }

        #endregion IDisposable
    }
}