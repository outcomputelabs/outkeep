using Orleans.Statistics;

namespace Outkeep.Grains.Tests.Fakes
{
    public class FakeHostEnvironmentStatistics : IHostEnvironmentStatistics
    {
        public long? TotalPhysicalMemory { get; set; }

        public float? CpuUsage { get; set; }

        public long? AvailableMemory { get; set; }
    }
}