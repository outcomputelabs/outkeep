using Orleans;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Outkeep.Grains.Tests.Fakes
{
    public class FakeTimerEntry
    {
        public FakeTimerEntry(Grain grain, Func<object?, Task> asyncCallback, object? state, TimeSpan dueTime, TimeSpan period, TaskScheduler taskScheduler)
        {
            Grain = grain;
            AsyncCallback = asyncCallback;
            State = state;
            DueTime = dueTime;
            Period = period;
            TaskScheduler = taskScheduler;
        }

        public Grain Grain { get; }
        public Func<object?, Task> AsyncCallback { get; }
        public object? State { get; }
        public TimeSpan DueTime { get; }
        public TimeSpan Period { get; }
        public TaskScheduler TaskScheduler { get; }

        public Task TickAsync()
        {
            return Task.Factory.StartNew(AsyncCallback, State, CancellationToken.None, TaskCreationOptions.DenyChildAttach, TaskScheduler).Unwrap();
        }
    }
}