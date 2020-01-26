using Outkeep.Governance;

namespace Outkeep.Grains.Governance
{
    public class ActivityState : IWeakActivationFactor
    {
        public ActivityPriority Priority { get; set; } = ActivityPriority.Normal;
    }
}