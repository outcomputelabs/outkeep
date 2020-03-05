using Outkeep.Timers;
using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Threading.Tasks;

namespace Outkeep.Core.Tests.Fakes
{
    public class FakeSafeTimerFactory : ISafeTimerFactory
    {
        public IDisposable Create(Func<object?, Task> callback, object? state, TimeSpan dueTime, TimeSpan period)
        {
            var timer = new FakeSafeTimer(callback, state, dueTime, period);
            Timers.Add(timer);

            return Disposable.Create(() => Timers.Remove(timer));
        }

        public ISet<FakeSafeTimer> Timers { get; } = new HashSet<FakeSafeTimer>();
    }
}