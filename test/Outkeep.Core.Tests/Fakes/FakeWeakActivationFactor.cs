using Outkeep.Governance;
using System;

namespace Outkeep.Core.Tests.Fakes
{
    public sealed class FakeWeakActivationFactor : IWeakActivationFactor
    {
        public int FakeProperty { get; set; }

        public int CompareTo(IWeakActivationFactor other)
        {
            return other is FakeWeakActivationFactor factor
                ? FakeProperty.CompareTo(factor.FakeProperty)
                : throw new InvalidOperationException();
        }

        public override bool Equals(object? obj)
        {
            return obj is FakeWeakActivationFactor factor
                && FakeProperty.Equals(factor.FakeProperty);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(FakeProperty);
        }

        public static bool operator ==(FakeWeakActivationFactor left, FakeWeakActivationFactor right)
        {
            return left is null ? right is null : left.Equals(right);
        }

        public static bool operator !=(FakeWeakActivationFactor left, FakeWeakActivationFactor right)
        {
            return !(left == right);
        }

        public static bool operator <(FakeWeakActivationFactor left, FakeWeakActivationFactor right)
        {
            return left is null ? right is object : left.CompareTo(right) < 0;
        }

        public static bool operator <=(FakeWeakActivationFactor left, FakeWeakActivationFactor right)
        {
            return left is null || left.CompareTo(right) <= 0;
        }

        public static bool operator >(FakeWeakActivationFactor left, FakeWeakActivationFactor right)
        {
            return left is object && left.CompareTo(right) > 0;
        }

        public static bool operator >=(FakeWeakActivationFactor left, FakeWeakActivationFactor right)
        {
            return left is null ? right is null : left.CompareTo(right) >= 0;
        }
    }
}