using Orleans.Concurrency;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Outkeep.Caching
{
    [Immutable]
    public readonly struct CachePulse : IEquatable<CachePulse>
    {
        public CachePulse(Guid tag, byte[]? value)
        {
            Tag = tag;
            Value = value;
        }

        public Guid Tag { get; }

        [SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "DTO")]
        public byte[]? Value { get; }

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
            return Tag == other.Tag
                && Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is CachePulse other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Tag, Value);
        }

        /// <summary>
        /// Returns a cache pulse with an empty tag and null value.
        /// </summary>
        public static CachePulse None { get; } = new CachePulse(Guid.Empty, null);

        /// <summary>
        /// Creates a new cache pulse with a random tag and null value.
        /// </summary>
        public static CachePulse RandomNull() => new CachePulse(Guid.NewGuid(), null);

        /// <summary>
        /// Creates a new cache pulse with a random tag and the provided value.
        /// </summary>
        public static CachePulse Random(byte[]? value) => new CachePulse(Guid.NewGuid(), value);
    }
}