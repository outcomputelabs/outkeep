using System;

namespace Outkeep.Core.Caching
{
    public readonly struct CacheEvictionArgs<TKey> : IEquatable<CacheEvictionArgs<TKey>> where TKey : notnull
    {
        public CacheEvictionArgs(ICacheEntry<TKey> cacheEntry, EvictionCause evictionCause)
        {
            CacheEntry = cacheEntry;
            EvictionCause = evictionCause;
        }

        public ICacheEntry<TKey> CacheEntry { get; }
        public EvictionCause EvictionCause { get; }

        public bool Equals(CacheEvictionArgs<TKey> other)
        {
            return CacheEntry == other.CacheEntry
                && EvictionCause == other.EvictionCause;
        }

        public override bool Equals(object obj)
        {
            return obj is CacheEvictionArgs<TKey> other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(CacheEntry, EvictionCause);
        }

        public static bool operator ==(CacheEvictionArgs<TKey> left, CacheEvictionArgs<TKey> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(CacheEvictionArgs<TKey> left, CacheEvictionArgs<TKey> right)
        {
            return !(left == right);
        }
    }
}