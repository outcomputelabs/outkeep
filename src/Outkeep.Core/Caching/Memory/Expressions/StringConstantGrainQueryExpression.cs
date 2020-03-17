using Orleans.Concurrency;
using ProtoBuf;

namespace Outkeep.Caching.Memory.Expressions
{
    [Immutable]
    [ProtoContract]
    internal class StringConstantGrainQueryExpression : ConstantGrainQueryExpression
    {
        protected StringConstantGrainQueryExpression()
        {
        }

        public StringConstantGrainQueryExpression(string? value)
        {
            Value = value;
        }

        [ProtoMember(1)]
        public string? Value { get; protected set; }
    }
}