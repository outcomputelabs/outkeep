using ProtoBuf;

namespace Outkeep.Caching.Memory
{
    [ProtoContract]
    internal class KeyEqualsGrainQueryCriterion : GrainQueryCriterion
    {
        protected KeyEqualsGrainQueryCriterion()
        {
        }

        public KeyEqualsGrainQueryCriterion(string? value)
        {
            Value = value;
        }

        [ProtoMember(1)]
        public string? Value { get; protected set; }
    }
}