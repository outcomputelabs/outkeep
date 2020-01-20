using ProtoBuf;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Outkeep.Grains
{
    [ProtoContract]
    internal class CacheGrainState
    {
        [ProtoMember(1)]
        public Guid Tag { get; set; }

        [ProtoMember(2)]
        [SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "State")]
        public byte[]? Value { get; set; }

        [ProtoMember(3)]
        public DateTimeOffset? AbsoluteExpiration { get; set; }

        [ProtoMember(4)]
        public TimeSpan? SlidingExpiration { get; set; }
    }
}