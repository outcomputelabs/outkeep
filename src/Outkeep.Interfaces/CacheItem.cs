using Orleans.Concurrency;
using System;
using System.Collections.Immutable;

namespace Outkeep.Interfaces
{
    [Immutable]
    public readonly struct CacheItem : IEquatable<CacheItem>
    {
        public CacheItem(ImmutableArray<byte> value, DateTimeOffset? absoluteExpiration, TimeSpan? slidingExpiration)
        {
            Value = value;
            AbsoluteExpiration = absoluteExpiration;
            SlidingExpiration = slidingExpiration;
        }

        public ImmutableArray<byte> Value { get; }
        public DateTimeOffset? AbsoluteExpiration { get; }
        public TimeSpan? SlidingExpiration { get; }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (obj is CacheItem other) return Equals(other);
            return false;
        }

        public override int GetHashCode() => HashCode.Combine(Value, AbsoluteExpiration, SlidingExpiration);

        public static bool operator ==(CacheItem left, CacheItem right)
        {
            return left.Value == right.Value
                && left.AbsoluteExpiration == right.AbsoluteExpiration
                && left.SlidingExpiration == right.SlidingExpiration;
        }

        public static bool operator !=(CacheItem left, CacheItem right)
        {
            return left.Value != right.Value
                || left.AbsoluteExpiration != right.AbsoluteExpiration
                || left.SlidingExpiration != right.SlidingExpiration;
        }

        public bool Equals(CacheItem other)
        {
            return this == other;
        }
    }
}