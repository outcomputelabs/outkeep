using ProtoBuf;
using System;

namespace Outkeep.Grains
{
    [ProtoContract]
    internal class CacheGrainFlags
    {
        [ProtoMember(1)]
        public DateTimeOffset UtcLastAccessed { get; set; } = DateTimeOffset.MinValue;
    }
}