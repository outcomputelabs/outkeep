using ProtoBuf;
using System;

namespace Outkeep.Grains
{
    [ProtoContract]
    internal class CacheGrainFlags
    {
        private DateTimeOffset _utcLastAccessed;

        [ProtoMember(1)]
        public DateTimeOffset UtcLastAccessed
        {
            get
            {
                return _utcLastAccessed;
            }
            set
            {
                _utcLastAccessed = value;
                HasChanged = true;
            }
        }

        [ProtoIgnore]
        public bool HasChanged { get; set; }
    }
}