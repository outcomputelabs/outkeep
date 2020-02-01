using System;
using System.Threading.Tasks;

namespace Outkeep.Grains.Tests.Fakes
{
    public sealed class FakeSafeTimer : IDisposable
    {
        public FakeSafeTimer(Func<object?, Task> callback, object? state, TimeSpan dueTime, TimeSpan period)
        {
            Callback = callback;
            State = state;
            DueTime = dueTime;
            Period = period;
        }

        public Func<object?, Task> Callback { get; }
        public object? State { get; }
        public TimeSpan DueTime { get; }
        public TimeSpan Period { get; }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        ~FakeSafeTimer()
        {
            Dispose();
        }
    }
}