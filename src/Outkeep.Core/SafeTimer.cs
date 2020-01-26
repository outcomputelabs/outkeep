using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Outkeep
{
    /// <summary>
    /// A rudimentary non-reentrant timer.
    /// </summary>
    public sealed class SafeTimer : IDisposable
    {
        private readonly Func<object?, Task> _callback;
        private readonly TimeSpan _period;
        private readonly Timer _timer;

        public SafeTimer(Func<object?, Task> callback, object? state, TimeSpan dueTime, TimeSpan period)
        {
            _callback = callback ?? throw new ArgumentNullException(nameof(callback));
            _period = period;

            // initialize the timer without ticking
            _timer = new Timer(Tick, state, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);

            // schedule the first tick
            Schedule(dueTime);
        }

        private void Schedule(TimeSpan wait)
        {
            _timer.Change(wait, Timeout.InfiniteTimeSpan);
        }

        [SuppressMessage("Major Bug", "S3168:\"async\" methods should not return \"void\"")]
        private async void Tick(object? state)
        {
            try
            {
                await _callback(state).ConfigureAwait(false);
            }
            finally
            {
                Schedule(_period);
            }
        }

        #region Disposable

        private bool _disposed;

        public void Dispose()
        {
            if (_disposed) return;

            _timer.Dispose();

            _disposed = true;

            GC.SuppressFinalize(this);
        }

        ~SafeTimer()
        {
            Dispose();
        }

        #endregion Disposable
    }
}