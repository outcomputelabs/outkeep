using System;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Outkeep.Caching.Memory
{
    // todo: implement on dispose to return this object to the pool
    internal sealed class RegistryQueryTranslator : ExpressionVisitor, IDisposable
    {
        private readonly static MethodInfo QueryableWhereMethod = typeof(Queryable).GetMethod(nameof(Queryable.Where), new[] { typeof(IQueryable<>), typeof(Expression<>) });

        private readonly ImmutableList<GrainQueryCriterion>.Builder _criteria = ImmutableList.CreateBuilder<GrainQueryCriterion>();

        public GrainQuery Translate(Expression expression)
        {
            Visit(expression);

            var result = new GrainQuery(_criteria.ToImmutable());

            return result;
        }

        public override Expression Visit(Expression node)
        {
            //throw new NotSupportedException();

            return base.Visit(node);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.Name == nameof(Queryable.Where) && node.Method.DeclaringType == typeof(Queryable))
            {
                Visit(node.Arguments);
            }

            throw new NotSupportedException();

            return base.VisitMethodCall(node);
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            return base.VisitLambda(node);
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return base.VisitParameter(node);
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