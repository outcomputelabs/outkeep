using Orleans.Concurrency;
using ProtoBuf;

namespace Outkeep.Caching.Memory.Expressions
{
    [Immutable]
    [ProtoContract]
    internal class UndefinedGrainQueryExpression : GrainQueryExpression
    {
        protected UndefinedGrainQueryExpression()
        {
        }

        public static readonly UndefinedGrainQueryExpression Default = new UndefinedGrainQueryExpression();
    }
}