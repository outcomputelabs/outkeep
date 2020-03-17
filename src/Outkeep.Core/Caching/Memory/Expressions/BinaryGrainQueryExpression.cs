using Orleans.Concurrency;
using ProtoBuf;
using System;

namespace Outkeep.Caching.Memory.Expressions
{
    [Immutable]
    [ProtoContract]
    internal class BinaryGrainQueryExpression : GrainQueryExpression
    {
        protected BinaryGrainQueryExpression()
        {
            Left = UndefinedGrainQueryExpression.Default;
            Right = UndefinedGrainQueryExpression.Default;
            Type = GrainQueryExpressionType.Undefined;
        }

        public BinaryGrainQueryExpression(GrainQueryExpression left, GrainQueryExpression right, GrainQueryExpressionType type)
        {
            Left = left ?? throw new ArgumentNullException(nameof(left));
            Right = right ?? throw new ArgumentNullException(nameof(right));
            Type = type;
        }

        [ProtoMember(1)]
        public GrainQueryExpression Left { get; protected set; }

        [ProtoMember(2)]
        public GrainQueryExpression Right { get; protected set; }

        [ProtoMember(3)]
        public GrainQueryExpressionType Type { get; protected set; }
    }
}