using Orleans.Concurrency;
using ProtoBuf;

namespace Outkeep.Caching.Memory.Expressions
{
    [Immutable]
    [ProtoContract]
    internal class RegistryGrainQueryExpression : ConstantGrainQueryExpression
    {
        protected RegistryGrainQueryExpression()
        {
        }

        public static readonly RegistryGrainQueryExpression Default = new RegistryGrainQueryExpression();
    }
}