using Orleans.Concurrency;
using ProtoBuf;
using System;
using static System.String;

namespace Outkeep.Caching.Memory
{
    [Immutable]
    [ProtoContract]
    internal class RegistryEntity
    {
        protected RegistryEntity()
        {
            Key = Empty;
        }

        public RegistryEntity(string key, int? size, DateTimeOffset? absoluteExpiration, TimeSpan? slidingExpiration, string? etag)
        {
            Key = key;
            Size = size;
            AbsoluteExpiration = absoluteExpiration;
            SlidingExpiration = slidingExpiration;
            ETag = etag;
        }

        [ProtoMember(1)]
        public string Key { get; protected set; }

        [ProtoMember(2)]
        public int? Size { get; protected set; }

        [ProtoMember(3)]
        public DateTimeOffset? AbsoluteExpiration { get; protected set; }

        [ProtoMember(4)]
        public TimeSpan? SlidingExpiration { get; protected set; }

        [ProtoMember(5)]
        public string? ETag { get; protected set; }
    }
}