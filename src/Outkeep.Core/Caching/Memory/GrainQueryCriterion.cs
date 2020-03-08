using ProtoBuf;

namespace Outkeep.Caching.Memory
{
    [ProtoContract]
    [ProtoInclude(1, typeof(KeyEqualsGrainQueryCriterion))]
    internal abstract class GrainQueryCriterion
    {
    }
}