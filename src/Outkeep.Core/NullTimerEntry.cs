using System;
using System.Threading.Tasks;

namespace Outkeep
{
    /// <summary>
    /// Holds timer information for the <see cref="NullTimerRegistry"/>.
    /// </summary>
    public sealed class NullTimerEntry : IDisposable
    {
        public NullTimerEntry(Func<object?, Task> asyncCallback, object? state, TimeSpan dueTime, TimeSpan period)
        {
            AsyncCallback = asyncCallback ?? throw new ArgumentNullException(nameof(asyncCallback));
            State = state;
            DueTime = dueTime;
            Period = period;
        }

        public Func<object, Task> AsyncCallback { get; }
        public object? State { get; }
        public TimeSpan DueTime { get; }
        public TimeSpan Period { get; }

        public bool IsDisposed { get; private set; }

        private void InnerDispose()
        {
            IsDisposed = true;
        }

        public void Dispose()
        {
            InnerDispose();
            GC.SuppressFinalize(this);
        }

        ~NullTimerEntry()
        {
            InnerDispose();
        }
    }
}