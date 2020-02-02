using Orleans;
using Orleans.Timers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading.Tasks;

namespace Outkeep.Grains.Tests.Fakes
{
    public class FakeTimerRegistry : ITimerRegistry
    {
        private readonly ConcurrentDictionary<FakeTimerEntry, bool> _entries = new ConcurrentDictionary<FakeTimerEntry, bool>();

        public IDisposable RegisterTimer(Grain grain, Func<object?, Task> asyncCallback, object? state, TimeSpan dueTime, TimeSpan period)
        {
            var entry = new FakeTimerEntry(grain, asyncCallback, state, dueTime, period, TaskScheduler.Current);

            _entries.TryAdd(entry, true);

            return Disposable.Create(() => _entries.TryRemove(entry, out _));
        }

        public IEnumerable<FakeTimerEntry> EnumerateEntries() => _entries.Select(x => x.Key);
    }
}