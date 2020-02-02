using System;
using System.Threading.Tasks;

namespace Outkeep.Timers
{
    public interface ISafeTimerFactory
    {
        IDisposable Create(Func<object?, Task> callback, object? state, TimeSpan dueTime, TimeSpan period);
    }
}