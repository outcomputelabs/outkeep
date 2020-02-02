namespace Outkeep.Governance.Memory
{
    public interface IMemoryPressureMonitor
    {
        bool IsUnderPressure { get; }
    }
}