using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans;
using Outkeep.Properties;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Outkeep.Governance.Memory
{
    public sealed class MemoryResourceGovernor : IHostedService, IResourceGovernor<ActivityState>, IDisposable
    {
        private readonly MemoryGovernanceOptions _options;
        private readonly ILogger _logger;
        private readonly IMemoryPressureMonitor _monitor;
        private readonly IGrainFactory _factory;

        public MemoryResourceGovernor(IOptions<MemoryGovernanceOptions> options, ILogger<MemoryResourceGovernor> logger, IMemoryPressureMonitor monitor, IGrainFactory factory)
        {
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _monitor = monitor ?? throw new ArgumentNullException(nameof(monitor));
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        private readonly ConcurrentDictionary<IGrainControlExtension, ActivityState> _registry = new ConcurrentDictionary<IGrainControlExtension, ActivityState>();
        private SafeTimer? _timer;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // todo: move the timespans to options
            _timer = new SafeTimer(TickGovern, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));

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

        private readonly List<IGrainControlExtension>[] _buckets = Enumerable.Range(1, 3).Select(x => new List<IGrainControlExtension>()).ToArray();

        private readonly List<Task> _deactivations = new List<Task>();

        [SuppressMessage("Critical Code Smell", "S1215:\"GC.Collect\" should not be called")]
        [SuppressMessage("Design", "CA1031:Do not catch general exception types")]
        private async Task TickGovern(object? state)
        {
            // quick break for no pressure
            if (!_monitor.IsUnderPressure) return;

            // quick break for nothing to deactivate
            var count = _registry.Count;
            if (count == 0) return;

            // calculate the quota to deactivate on this pass
            var quota = (int)Math.Ceiling(count * _options.GrainDeactivationRatio);

            // quick break for nothing to deactivate or invalid ratio
            if (quota <= 0) return;
            _logger.LogInformation(Resources.Log_AttemptingToDeactivate_X_GrainsOutOf_X_InResponseToMemoryPressure, quota, count);

            // deactivate subjects by priority
            foreach (var entry in _registry)
            {
                switch (entry.Value.Priority)
                {
                    case ActivityPriority.Low:
                        _buckets[0].Add(entry.Key);
                        break;

                    case ActivityPriority.Normal:
                        _buckets[1].Add(entry.Key);
                        break;

                    case ActivityPriority.High:
                        _buckets[2].Add(entry.Key);
                        break;
                }
            }

            for (var b = 0; b < _buckets.Length; ++b)
            {
                var bucket = _buckets[b];

                for (var i = 0; i < bucket.Count; ++i)
                {
                    if (quota-- == 0) break;

                    var entry = bucket[i];

                    if (_registry.TryRemove(entry, out _))
                    {
                        _deactivations.Add(entry.DeactivateOnIdleAsync());
                    }
                }
            }

            try
            {
                await Task.WhenAll(_deactivations).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, Resources.Log_FailedToDeactivateOneOrMoreTargetGrains);
            }

            for (var b = 0; b < _buckets.Length; ++b)
            {
                _buckets[b].Clear();
            }
            _deactivations.Clear();

            // test to see if we can do without this
            GC.Collect();
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