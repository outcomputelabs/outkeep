using System;
using System.Diagnostics.CodeAnalysis;

namespace Outkeep.Core.Storage
{
    /// <summary>
    /// Represents a cached item.
    /// </summary>
    public readonly struct CacheItem : IEquatable<CacheItem>
    {
        public CacheItem(byte[] value, DateTimeOffset? absoluteExpiration, TimeSpan? slidingExpiration)
        {
            Value = value;
            AbsoluteExpiration = absoluteExpiration;
            SlidingExpiration = slidingExpiration;
        }

        /// <summary>
        /// The cached binary payload.
        /// </summary>
        [SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "DTO")]
        public byte[] Value { get; }

        /// <summary>
        /// The absolute expiration schedule for this cache item.
        /// </summary>
        public DateTimeOffset? AbsoluteExpiration { get; }

        /// <summary>
        /// The sliding expiration period for this cache item.
        /// </summary>
        public TimeSpan? SlidingExpiration { get; }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (obj is CacheItem other) return Equals(other);
            return false;
        }

        public bool Equals(CacheItem other)
        {
            return Value == other.Value
                && AbsoluteExpiration == other.AbsoluteExpiration
                && SlidingExpiration == other.SlidingExpiration;
        }

        public override int GetHashCode() => HashCode.Combine(Value, AbsoluteExpiration, SlidingExpiration);

        public static bool operator ==(CacheItem left, CacheItem right) => left.Equals(right);

        public static bool operator !=(CacheItem left, CacheItem right) => !left.Equals(right);
    }
}