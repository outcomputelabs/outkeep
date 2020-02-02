using Orleans;
using Orleans.Runtime;
using System;
using System.Collections.Generic;
using System.Reactive.Disposables;

namespace Outkeep.Grains.Tests.Fakes
{
    public class FakeGrainLifecycle : IGrainLifecycle
    {
        public ISet<(string ObserverName, int Stage, ILifecycleObserver observer)> Subscriptions { get; } = new HashSet<(string ObserverName, int Stage, ILifecycleObserver observer)>();

        public IDisposable Subscribe(string observerName, int stage, ILifecycleObserver observer)
        {
            Subscriptions.Add((observerName, stage, observer));

            return Disposable.Create(() => Subscriptions.Remove((observerName, stage, observer)));
        }
    }
}