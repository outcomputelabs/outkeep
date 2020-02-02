using Orleans;
using Orleans.Timers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Outkeep
{
    /// <summary>
    /// Implements an <see cref="ITimerRegistry"/> that does nothing on its own and instead allows access to the registered timers for manual invocation.
    /// Use this for testing where you do not want a mock or to disable the timer registry in a deployment.
    /// This class is thread-safe.
    /// </summary>
    public class NullTimerRegistry : ITimerRegistry
    {
        private readonly ConcurrentDictionary<Grain, ImmutableList<NullTimerEntry>> _entries =
            new ConcurrentDictionary<Grain, ImmutableList<NullTimerEntry>>();

        public IDisposable RegisterTimer(Grain grain, Func<object?, Task> asyncCallback, object? state, TimeSpan dueTime, TimeSpan period)
        {
            var timer = new NullTimerEntry(asyncCallback, state, dueTime, period);

            _entries.AddOrUpdate(grain,
                (k, t) => ImmutableList.Create(t),
                (k, v, t) => v.Add(t),
                timer);

            return timer;
        }

        public IEnumerable<NullTimerEntry> GetEntries(Grain grain) =>
            _entries.TryGetValue(grain, out var list) ? list : ImmutableList<NullTimerEntry>.Empty;
    }
}