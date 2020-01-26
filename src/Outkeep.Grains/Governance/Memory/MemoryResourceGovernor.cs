using Microsoft.Extensions.Hosting;
using Orleans;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Outkeep.Governance.Memory
{
    public sealed class MemoryResourceGovernor : IHostedService, IResourceGovernor<ActivityState>, IDisposable
    {
        private readonly IMemoryPressureMonitor _monitor;
        private readonly IGrainFactory _factory;

        public MemoryResourceGovernor(IMemoryPressureMonitor monitor, IGrainFactory factory)
        {
            _monitor = monitor ?? throw new ArgumentNullException(nameof(monitor));
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        private readonly ConcurrentDictionary<IGrainControlExtension, ActivityState> _registry = new ConcurrentDictionary<IGrainControlExtension, ActivityState>();
        private Timer? _timer;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // todo: move the timespans to options
            _timer = new Timer(TickGovern, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            if (_timer != null)
            {
                _timer.Dispose();
                _timer = null;
            }

            return Task.CompletedTask;
        }

        public Task EnlistAsync(IGrainControlExtension subject, ActivityState state)
        {
            _registry[subject] = state;

            return Task.CompletedTask;
        }

        public Task LeaveAsync(IGrainControlExtension subject)
        {
            _registry.TryRemove(subject, out _);

            return Task.CompletedTask;
        }

        private void TickGovern(object state)
        {
            if (_monitor.IsUnderPressure)
            {
                // todo: do some governing here
            }
        }

        #region Disposable

        public void Dispose()
        {
            InnerDispose();
            GC.SuppressFinalize(this);
        }

        private void InnerDispose()
        {
            if (_timer != null)
            {
                _timer.Dispose();
                _timer = null;
            }
        }

        ~MemoryResourceGovernor()
        {
            InnerDispose();
        }

        #endregion Disposable
    }
}