using Orleans.Concurrency;
using ProtoBuf;
using System;
using System.Collections.Immutable;
using static System.String;

namespace Outkeep.Caching.Memory.Expressions
{
    [Immutable]
    [ProtoContract]
    internal class MethodCallGrainQueryExpression : GrainQueryExpression
    {
        protected MethodCallGrainQueryExpression()
        {
            DeclaringType = Empty;
            Name = Empty;
        }

        public MethodCallGrainQueryExpression(string declaringType, string name)
        {
            DeclaringType = declaringType ?? throw new ArgumentNullException(nameof(declaringType));
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        [ProtoMember(1)]
        public string DeclaringType { get; protected set; }

        [ProtoMember(2)]
        public string Name { get; protected set; }

        [ProtoMember(3, OverwriteList = true)]
        public ImmutableList<GrainQueryExpression> Arguments { get; protected set; }
    }
}