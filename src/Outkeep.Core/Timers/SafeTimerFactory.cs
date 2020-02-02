using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Outkeep.Timers
{
    internal class SafeTimerFactory : ISafeTimerFactory
    {
        private readonly ILogger<SafeTimer> _timerLogger;

        public SafeTimerFactory(ILogger<SafeTimer> timerLogger)
        {
            _timerLogger = timerLogger ?? throw new ArgumentNullException(nameof(timerLogger));
        }

        public IDisposable Create(Func<object?, Task> callback, object? state, TimeSpan dueTime, TimeSpan period)
        {
            return new SafeTimer(_timerLogger, callback, state, dueTime, period);
        }
    }
}