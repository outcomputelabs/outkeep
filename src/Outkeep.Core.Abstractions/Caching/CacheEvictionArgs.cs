﻿using System;

namespace Outkeep.Core.Caching
{
    public struct CacheEvictionArgs : IEquatable<CacheEvictionArgs>
    {
        public CacheEvictionArgs(ICacheEntry cacheEntry, EvictionCause evictionCause)
        {
            CacheEntry = cacheEntry;
            EvictionCause = evictionCause;
        }

        public ICacheEntry CacheEntry { get; }
        public EvictionCause EvictionCause { get; }

        public bool Equals(CacheEvictionArgs other)
        {
            return CacheEntry == other.CacheEntry
                && EvictionCause == other.EvictionCause;
        }

        public override bool Equals(object obj)
        {
            return obj is CacheEvictionArgs other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(CacheEntry, EvictionCause);
        }

        public static bool operator ==(CacheEvictionArgs left, CacheEvictionArgs right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(CacheEvictionArgs left, CacheEvictionArgs right)
        {
            return !(left == right);
        }
    }
}