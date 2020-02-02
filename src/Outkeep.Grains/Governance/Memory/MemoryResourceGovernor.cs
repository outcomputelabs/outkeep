using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Outkeep.Properties;
using Outkeep.Timers;
using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Outkeep.Governance.Memory
{
    public sealed class MemoryResourceGovernor : IHostedService, IResourceGovernor, IDisposable
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

        private readonly ConcurrentDictionary<IWeakActivationExtension, Entry> _registry = new ConcurrentDictionary<IWeakActivationExtension, Entry>();
        private IDisposable? _timer;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = _timers.Create(_ => TickGovern(), null, _options.WeakActivationCollectionInterval, _options.WeakActivationCollectionInterval);

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

        public Task EnlistAsync(IWeakActivationExtension subject, IWeakActivationFactor factor)
        {
            if (subject is null) throw new ArgumentNullException(nameof(subject));
            if (factor is null) throw new ArgumentNullException(nameof(factor));

            _registry[subject] = new Entry(factor);

            return Task.CompletedTask;
        }

        public Task LeaveAsync(IWeakActivationExtension subject)
        {
            _registry.TryRemove(subject, out _);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Checks if the given subject is enlisted.
        /// </summary>
        public bool IsEnlisted(IWeakActivationExtension subject)
        {
            return _registry.ContainsKey(subject);
        }

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

        [SuppressMessage("Design", "CA1031:Do not catch general exception types")]
        private async Task DeactivateByPriorityAsync(int quota)
        {
            // quick path for quota complete
            if (quota <= 0) return;

            // deactivate subjects by priority
            foreach (var entry in _registry.OrderBy(x => x.Value.Factor).Take(quota))
            {
                bool deactivated;
                try
                {
                    await entry.Key.DeactivateOnIdleAsync().ConfigureAwait(false);
                    deactivated = true;
                }
                catch (Exception ex)
                {
                    Log.FailedToDeactivateGrain(_logger, entry.Key, ex);
                    deactivated = false;
                }

                if (deactivated)
                {
                    _registry.TryRemove(entry.Key, out _);
                }
                else
                {
                    entry.Value.DeactivationAttempts += 1;
                    if (entry.Value.DeactivationAttempts >= _options.MaxGrainDeactivationAttempts)
                    {
                        Log.FailedToDeactivateGrainWillNotRetry(_logger, entry.Key, entry.Value.DeactivationAttempts);
                        _registry.TryRemove(entry.Key, out _);
                    }
                }
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

        private static class Log
        {
            public static void FailedToDeactivateGrain(ILogger logger, IWeakActivationExtension target, Exception exception) =>
                FailedToDeactivateGrainAction(logger, target, exception);

            private static readonly Action<ILogger, IWeakActivationExtension, Exception> FailedToDeactivateGrainAction =
                LoggerMessage.Define<IWeakActivationExtension>(LogLevel.Error, new EventId(1, nameof(FailedToDeactivateGrain)), Resources.Log_FailedToDeactivateTargetGrain_X);

            public static void FailedToDeactivateGrainWillNotRetry(ILogger logger, IWeakActivationExtension target, int attempts) =>
                FailedToDeactivateGrainWillNotRetryAction(logger, target, attempts, null!);

            private static readonly Action<ILogger, IWeakActivationExtension, int, Exception> FailedToDeactivateGrainWillNotRetryAction =
                LoggerMessage.Define<IWeakActivationExtension, int>(LogLevel.Warning, new EventId(2, nameof(FailedToDeactivateGrainWillNotRetry)), Resources.Log_FailedToDeactivateTargetGrain_X_After_X_AttemptsWillNotRetry);
        }

        private class Entry
        {
            public Entry(IWeakActivationFactor factor)
            {
                Factor = factor ?? throw new ArgumentNullException(nameof(factor));
            }

            public IWeakActivationFactor Factor { get; }
            public int DeactivationAttempts { get; set; }
        }
    }
}