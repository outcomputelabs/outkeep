namespace Outkeep.Governance
{
    public class ActivityState : IWeakActivationFactor
    {
        public ActivityPriority Priority { get; set; } = ActivityPriority.Normal;
    }
}