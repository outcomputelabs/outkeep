namespace Outkeep.Core.Governance
{
    public interface IMemoryPressureMonitor
    {
        bool IsUnderPressure { get; }
    }
}