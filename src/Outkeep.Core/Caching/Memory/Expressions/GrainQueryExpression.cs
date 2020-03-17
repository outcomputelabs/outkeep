using Orleans.Concurrency;
using ProtoBuf;

namespace Outkeep.Caching.Memory.Expressions
{
    [Immutable]
    [ProtoContract]
    [ProtoInclude(1, typeof(UndefinedGrainQueryExpression))]
    [ProtoInclude(2, typeof(MethodCallGrainQueryExpression))]
    [ProtoInclude(3, typeof(ConstantGrainQueryExpression))]
    [ProtoInclude(4, typeof(BinaryGrainQueryExpression))]
    internal abstract class GrainQueryExpression
    {
    }
}