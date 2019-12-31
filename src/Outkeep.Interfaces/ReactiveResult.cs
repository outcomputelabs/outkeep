using Orleans.Concurrency;
using System;

namespace Outkeep.Interfaces
{
    [Immutable]
    public struct ReactiveResult<TETag, TValue> : IEquatable<ReactiveResult<TETag, TValue>>
    {
        public ReactiveResult(TETag etag, TValue value)
        {
            ETag = etag;
            Value = value;
        }

        public TETag ETag { get; }

        public TValue Value { get; }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (obj is ReactiveResult<TETag, TValue> other) return Equals(other);
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Value, ETag);
        }

        public static bool operator ==(ReactiveResult<TETag, TValue> left, ReactiveResult<TETag, TValue> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ReactiveResult<TETag, TValue> left, ReactiveResult<TETag, TValue> right)
        {
            return !(left == right);
        }

        public bool Equals(ReactiveResult<TETag, TValue> other)
        {
            return (ETag is null ? other.ETag is null : ETag.Equals(other.ETag))
                && (Value is null ? other.Value is null : Value.Equals(other.Value));
        }
    }
}