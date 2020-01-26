using Microsoft.Extensions.Options;
using Orleans.Statistics;
using System;

namespace Outkeep.Governance.Memory
{
    public class MemoryPressureMonitor : IMemoryPressureMonitor
    {
        private readonly IHostEnvironmentStatistics _stats;
        private readonly MemoryGovernanceOptions _options;

        public MemoryPressureMonitor(IHostEnvironmentStatistics stats, IOptions<MemoryGovernanceOptions> options)
        {
            _stats = stats ?? throw new ArgumentNullException(nameof(stats));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        }

        public bool IsUnderPressure
        {
            get
            {
                // quick path for no available memory stats
                var available = _stats.AvailableMemory;
                if (!available.HasValue)
                {
                    return false;
                }

                // memory is under pressure if below the low bytes threshold
                if (_options.LowMemoryBytesThreshold.HasValue && available.Value < _options.LowMemoryBytesThreshold.Value)
                {
                    return true;
                }

                // quick path for no total memory stats or div by zero
                var total = _stats.TotalPhysicalMemory;
                if (!total.HasValue || total.Value == 0)
                {
                    return false;
                }

                // memory is under pressure if ratio is below the threshold
                if (_options.LowMemoryThreshold.HasValue && available.Value / total.Value < _options.LowMemoryThreshold)
                {
                    return true;
                }

                // all good
                return false;
            }
        }
    }
}