using System;
using System.Threading.Tasks;

namespace Outkeep
{
    public interface ISafeTimerFactory
    {
        IDisposable Create(Func<object?, Task> callback, object? state, TimeSpan dueTime, TimeSpan period);
    }
}