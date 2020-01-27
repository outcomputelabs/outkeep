using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Outkeep
{
    /// <summary>
    /// A rudimentary non-reentrant timer.
    /// This timer skips ticks if a prior tick is still running.
    /// </summary>
    public sealed class SafeTimer : IDisposable
    {
        private readonly ILogger _logger;
        private readonly Func<object?, Task> _callback;
        private readonly Timer _timer;

        public SafeTimer(ILogger<SafeTimer> logger, Func<object?, Task> callback, object? state, TimeSpan dueTime, TimeSpan period)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _callback = callback ?? throw new ArgumentNullException(nameof(callback));

            _timer = new Timer(Tick, state, dueTime, period);
        }

        private int _running;

        private void Tick(object? state)
        {
            // skip the tick if the previous one is still running
            // otherwise flag the timer as running
            if (Interlocked.CompareExchange(ref _running, 1, 0) != 0) return;

            // run this tick and unflag the timer at the end
            _callback(state).ContinueWith(t => _running = 0, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
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