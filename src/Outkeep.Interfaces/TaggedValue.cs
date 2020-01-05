using Orleans.Concurrency;
using System;

namespace Outkeep.Interfaces
{
    /// <summary>
    /// Associates a value with an etag or version identifier.
    /// For use with long polling and reactive caching grain methods.
    /// </summary>
    /// <typeparam name="TETag">The entity tag or version identifier of the associated value.</typeparam>
    /// <typeparam name="TValue">The value.</typeparam>
    [Immutable]
    public struct TaggedValue<TETag, TValue> : IEquatable<TaggedValue<TETag, TValue>>
    {
        public TaggedValue(TETag etag, TValue value)
        {
            ETag = etag;
            Value = value;
        }

        public TETag ETag { get; }

        public TValue Value { get; }

        public static bool operator ==(TaggedValue<TETag, TValue> left, TaggedValue<TETag, TValue> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TaggedValue<TETag, TValue> left, TaggedValue<TETag, TValue> right)
        {
            return !(left == right);
        }

        public bool Equals(TaggedValue<TETag, TValue> other)
        {
            return (ETag is null ? other.ETag is null : ETag.Equals(other.ETag))
                && (Value is null ? other.Value is null : Value.Equals(other.Value));
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (obj is TaggedValue<TETag, TValue> other) return Equals(other);
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Value, ETag);
        }
    }
}