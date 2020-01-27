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
        private readonly ISafeTimerFactory _timers;

        public MemoryResourceGovernor(IOptions<MemoryGovernanceOptions> options, ILogger<MemoryResourceGovernor> logger, IMemoryPressureMonitor monitor, ISafeTimerFactory timers)
        {
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _monitor = monitor ?? throw new ArgumentNullException(nameof(monitor));
            _timers = timers ?? throw new ArgumentNullException(nameof(timers));
        }

        private readonly ConcurrentDictionary<IWeakActivationExtension, ActivityState> _registry = new ConcurrentDictionary<IWeakActivationExtension, ActivityState>();
        private IDisposable? _timer;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = _timers.Create(_ => TickGovern(), null, _options.ActivationCollectionInterval, _options.ActivationCollectionInterval);

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

        public Task EnlistAsync(IWeakActivationExtension subject, ActivityState state)
        {
            _registry[subject] = state;

            return Task.CompletedTask;
        }

        public Task LeaveAsync(IWeakActivationExtension subject)
        {
            _registry.TryRemove(subject, out _);

            return Task.CompletedTask;
        }

        private readonly List<Task> _deactivations = new List<Task>();

        [SuppressMessage("Critical Code Smell", "S1215:\"GC.Collect\" should not be called")]
        private async Task TickGovern()
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

            // apply deactivation rules
            await DeactivateByPriorityAsync(quota).ConfigureAwait(false);

            // test to see if we can do without this
            GC.Collect();
        }

        private readonly List<IWeakActivationExtension>[] _buckets = Enumerable.Range(1, 3).Select(x => new List<IWeakActivationExtension>()).ToArray();

        [SuppressMessage("Design", "CA1031:Do not catch general exception types")]
        private async Task<long> DeactivateByPriorityAsync(long quota)
        {
            // quick path for quota complete
            if (quota <= 0) return quota;

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
                        _deactivations.Add(entry.AsReference<IWeakActivationExtension>().DeactivateOnIdleAsync());
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

            return quota;
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