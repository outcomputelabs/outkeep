using System;
using System.Collections.Immutable;
using System.Linq.Expressions;

namespace Outkeep.Caching.Memory
{
    // todo: implement on dispose to return this object to the pool
    internal sealed class RegistryQueryTranslator : ExpressionVisitor, IDisposable
    {
        private readonly ImmutableList<GrainQueryCriterion>.Builder _criteria = ImmutableList.CreateBuilder<GrainQueryCriterion>();

        public GrainQuery Translate(Expression expression)
        {
            Visit(expression);

            var result = new GrainQuery(_criteria.ToImmutable());

            return result;
        }

        // todo: call this from the object pool
        public void Reset()
        {
            _criteria.Clear();
        }

        public void Dispose()
        {
            _criteria.Clear();

            // todo: return this object to the pool here
        }
    }
}