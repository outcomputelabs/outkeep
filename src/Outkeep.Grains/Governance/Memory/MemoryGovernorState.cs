namespace Outkeep.Grains.Governance.Memory
{
    public class MemoryGovernorState
    {
        /// <inheritdoc />
        public bool IsBusy { get; set; } = false;

        /// <inheritdoc />
        public ActivityPriority Priority { get; set; } = ActivityPriority.Normal;

        /// <inheritdoc />
        public long Size { get; set; } = 0L;
    }
}