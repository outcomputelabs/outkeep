using System;
using System.Threading.Tasks;

namespace Outkeep.Core.Caching
{
    internal sealed class PostEvictionCallbackRegistration : IDisposable
    {
        public PostEvictionCallbackRegistration(Action<object?> callback, object? state, TaskScheduler taskScheduler, ICacheEntryContext context)
        {
            Callback = callback ?? throw new ArgumentNullException(nameof(callback));
            State = state;
            TaskScheduler = taskScheduler ?? throw new ArgumentNullException(nameof(taskScheduler));

            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Context used for notifications.
        /// </summary>
        private readonly ICacheEntryContext _context;

        /// <summary>
        /// The user callback to invoke upon eviction.
        /// </summary>
        public Action<object?> Callback { get; }

        /// <summary>
        /// The user supplied state to return on the callback.
        /// </summary>
        public object? State { get; }

        /// <summary>
        /// The task scheduler to use when scheduling the callback.
        /// This allows use of the orleans activation task scheduler of the grain instance that made this registration.
        /// </summary>
        public TaskScheduler TaskScheduler { get; }

        #region IDisposable

        public void Dispose()
        {
            _context.OnPostEvictionCallbackRegistrationDisposed(this);
            GC.SuppressFinalize(this);
        }

        ~PostEvictionCallbackRegistration()
        {
            Dispose();
        }

        #endregion IDisposable
    }
}