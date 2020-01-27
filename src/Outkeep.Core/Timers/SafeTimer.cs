using Microsoft.Extensions.Logging;
using Outkeep.Properties;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Outkeep.Timers
{
    /// <summary>
    /// A rudimentary non-reentrant timer.
    /// This timer skips ticks if a prior tick is still running.
    /// </summary>
    internal sealed class SafeTimer : IDisposable
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
            _callback(state)
                .ContinueWith(t =>
                {
                    if (t.IsCanceled)
                    {
                        Log.TickCancelled(_logger, t.Exception);
                    }
                    else if (t.IsFaulted)
                    {
                        Log.TickFaulted(_logger, t.Exception);
                    }

                    _running = 0;
                },
                CancellationToken.None,
                TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Default);
        }

        private static class Log
        {
            public static void TickCancelled(ILogger logger, Exception exception) =>
                _tickCancelled(logger, exception);

            private static readonly Action<ILogger, Exception> _tickCancelled =
                LoggerMessage.Define(LogLevel.Warning, new EventId(1), Resources.Log_TimerTickWasCancelled);

            public static void TickFaulted(ILogger logger, Exception exception) =>
                _tickFaulted(logger, exception);

            private static readonly Action<ILogger, Exception> _tickFaulted =
                LoggerMessage.Define(LogLevel.Warning, new EventId(2), Resources.Log_TimerTickHasFaulted);
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