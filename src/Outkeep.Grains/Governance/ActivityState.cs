using System;

namespace Outkeep.Governance
{
    public class ActivityState : IWeakActivationFactor
    {
        public ActivityPriority Priority { get; set; } = ActivityPriority.Normal;

        public int CompareTo(IWeakActivationFactor other)
        {
            return other is ActivityState activity
                ? Priority.CompareTo(activity.Priority)
                : throw new InvalidOperationException();
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return (obj is ActivityState other)
                && Priority == other.Priority;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Priority);
        }

        public static bool operator ==(ActivityState left, ActivityState right)
        {
            return left is null ? right is null : left.Equals(right);
        }

        public static bool operator !=(ActivityState left, ActivityState right)
        {
            return !(left == right);
        }

        public static bool operator <(ActivityState left, ActivityState right)
        {
            return left is null ? right is object : left.CompareTo(right) < 0;
        }

        public static bool operator <=(ActivityState left, ActivityState right)
        {
            return left is null || left.CompareTo(right) <= 0;
        }

        public static bool operator >(ActivityState left, ActivityState right)
        {
            return left is object && left.CompareTo(right) > 0;
        }

        public static bool operator >=(ActivityState left, ActivityState right)
        {
            return left is null ? right is null : left.CompareTo(right) >= 0;
        }
    }
}