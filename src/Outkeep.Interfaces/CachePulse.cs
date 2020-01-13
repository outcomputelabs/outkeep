using Orleans.Concurrency;
using System;

namespace Outkeep.Interfaces
{
    [Immutable]
    public readonly struct CachePulse : IEquatable<CachePulse>
    {
        public CachePulse(Guid tag, byte[]? value)
            : this(tag, new Immutable<byte[]?>(value))
        {
        }

        public CachePulse(Guid tag, Immutable<byte[]?> value)
        {
            Tag = tag;
            Value = value;
        }

        public Guid Tag { get; }

        public Immutable<byte[]?> Value { get; }

        public static bool operator ==(CachePulse left, CachePulse right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(CachePulse left, CachePulse right)
        {
            return !(left == right);
        }

        public bool Equals(CachePulse other)
        {
            return Tag.Equals(other.Tag)
                && Value.Equals(other.Value);
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (obj is CachePulse other) return Equals(other);
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Tag, Value);
        }

        /// <summary>
        /// Returns a cache pulse with an empty tag and default (null) value.
        /// </summary>
        public static CachePulse None { get; } = new CachePulse(Guid.Empty, new Immutable<byte[]?>(null));
    }
}