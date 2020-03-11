using ProtoBuf;
using static System.String;

namespace Outkeep.Caching.Memory
{
    [ProtoContract]
    [ProtoInclude(1, typeof(KeyEqualsCriterion))]
    internal abstract class GrainQueryCriterion
    {
    }

    [ProtoContract]
    internal class KeyEqualsCriterion : GrainQueryCriterion
    {
        protected KeyEqualsCriterion()
        {
            Value = Empty;
        }

        public KeyEqualsCriterion(string value)
        {
            Value = value;
        }

        [ProtoMember(1)]
        public string Value { get; protected set; }
    }
}