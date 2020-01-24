using Orleans.Concurrency;
using System;

namespace Outkeep.Grains
{
    /// <summary>
    /// Associates a value with an entity tag or version identifier.
    /// For use with long polling and reactive caching grain methods.
    /// </summary>
    /// <typeparam name="TTag">The entity tag or version identifier of the associated value.</typeparam>
    /// <typeparam name="TValue">The value.</typeparam>
    [Immutable]
    public readonly struct TaggedValue<TTag, TValue> : IEquatable<TaggedValue<TTag, TValue>>
    {
        public TaggedValue(TTag tag, TValue value)
        {
            Tag = tag;
            Value = value;
        }

        public TTag Tag { get; }

        public TValue Value { get; }

        public static bool operator ==(TaggedValue<TTag, TValue> left, TaggedValue<TTag, TValue> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TaggedValue<TTag, TValue> left, TaggedValue<TTag, TValue> right)
        {
            return !(left == right);
        }

        public bool Equals(TaggedValue<TTag, TValue> other)
        {
            return (Tag is null ? other.Tag is null : Tag.Equals(other.Tag))
                && (Value is null ? other.Value is null : Value.Equals(other.Value));
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (obj is TaggedValue<TTag, TValue> other) return Equals(other);
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Tag, Value);
        }
    }
}