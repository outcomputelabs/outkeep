using ProtoBuf;
using System;

namespace Outkeep.Caching
{
    [ProtoContract]
    internal class CacheGrainFlags
    {
        [ProtoMember(1)]
        public DateTimeOffset UtcLastAccessed { get; set; }

        [ProtoMember(2)]
        public DateTimeOffset PersistedUtcLastAccessed { get; set; }
    }
}