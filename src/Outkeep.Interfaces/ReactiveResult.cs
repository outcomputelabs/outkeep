using Orleans.Concurrency;
using System;

namespace Outkeep.Interfaces
{
    [Immutable]
    public class ReactiveResult<T>
    {
        public ReactiveResult(T value, Guid etag)
        {
            Value = value;
            ETag = etag;
        }

        public T Value { get; }
        public Guid ETag { get; }
    }
}