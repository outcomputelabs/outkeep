using ProtoBuf;
using System;
using System.Collections.Immutable;

namespace Outkeep.Caching.Memory
{
    /// <summary>
    /// A rudimentary query parameters message.
    /// This somewhat compensates for the current lack of serialization support for expression trees.
    /// </summary>
    [ProtoContract]
    internal class GrainQuery
    {
        protected GrainQuery()
        {
        }

        public GrainQuery(ImmutableList<GrainQueryCriterion> criteria)
        {
            Criteria = criteria ?? throw new ArgumentNullException(nameof(criteria));
        }

        [ProtoMember(1, OverwriteList = true)]
        public ImmutableList<GrainQueryCriterion> Criteria { get; protected set; } = ImmutableList<GrainQueryCriterion>.Empty;
    }
}