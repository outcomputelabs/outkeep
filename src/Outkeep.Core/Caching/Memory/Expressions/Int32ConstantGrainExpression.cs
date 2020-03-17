using Orleans.Concurrency;
using ProtoBuf;

namespace Outkeep.Caching.Memory.Expressions
{
    [Immutable]
    [ProtoContract]
    internal class Int32ConstantGrainExpression : ConstantGrainQueryExpression
    {
        protected Int32ConstantGrainExpression()
        {
        }

        public Int32ConstantGrainExpression(int? value)
        {
            Value = value;
        }

        [ProtoMember(1)]
        public int? Value { get; protected set; }
    }
}