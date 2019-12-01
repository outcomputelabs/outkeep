using ProtoBuf;
using System;

namespace Outkeep.Core
{
    [ProtoContract]
    internal class FileCacheItem
    {
        [ProtoMember(1)]
        public byte[] Value { get; set; }

        [ProtoMember(2)]
        public DateTimeOffset? AbsoluteExpiration { get; set; }

        [ProtoMember(3)]
        public TimeSpan? SlidingExpiration { get; set; }
    }
}