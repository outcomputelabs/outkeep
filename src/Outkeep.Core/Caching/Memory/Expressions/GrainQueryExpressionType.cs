using ProtoBuf;

namespace Outkeep.Caching.Memory.Expressions
{
    [ProtoContract]
    internal enum GrainQueryExpressionType
    {
        Undefined = 0,
        And = 1,
        Or = 2,
        Equal = 3,
        NotEqual = 4,
        LessThan = 5,
        LessThanOrEqual = 6,
        GreaterThan = 7,
        GreaterThanOrEqual = 8
    }
}