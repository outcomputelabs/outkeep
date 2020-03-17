using Orleans.Concurrency;
using ProtoBuf;

namespace Outkeep.Caching.Memory.Expressions
{
    [Immutable]
    [ProtoContract]
    [ProtoInclude(1, typeof(RegistryGrainQueryExpression))]
    [ProtoInclude(2, typeof(StringConstantGrainQueryExpression))]
    [ProtoInclude(3, typeof(Int32ConstantGrainExpression))]
    internal class ConstantGrainQueryExpression : GrainQueryExpression
    {
    }
}