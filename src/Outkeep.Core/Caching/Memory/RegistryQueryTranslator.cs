using Outkeep.Caching.Memory.Expressions;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace Outkeep.Caching.Memory
{
    // todo: implement on dispose to return this object to the pool
    internal sealed class RegistryQueryTranslator : ExpressionVisitor, IDisposable
    {
        private GrainQueryExpression _query;

        public GrainQueryExpression Translate(Expression expression)
        {
            _query = RegistryGrainQueryExpression.Default;

            Visit(expression);

            return _query;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            _query = new MethodCallGrainQueryExpression(node.Method.DeclaringType.FullName, node.Method.Name);

            if (node.Method.DeclaringType == typeof(Queryable) && node.Method.Name == nameof(Queryable.Where))
            {
                Visit(node.Arguments);

                return node;
            }

            throw new NotSupportedException();
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            switch (node.NodeType)
            {
                case ExpressionType.Equal:

                    // check for the key equals where
                    if (node.Left is MemberExpression member && typeof(ICacheRegistryEntry).IsAssignableFrom(member.Member.DeclaringType) && member.Member.Name == nameof(ICacheRegistryEntry.Key))
                    {
                        ConstantExpression constant;

                        if (node.Right is ConstantExpression c)
                        {
                            constant = c;
                        }
                        else if (node.Right is MemberExpression rightMember)
                        {
                            constant = Expression.Constant(Expression.Lambda(rightMember).Compile().DynamicInvoke(), rightMember.Type);
                        }
                        else
                        {
                            throw new NotSupportedException();
                        }

                        if (constant.Type == typeof(string))
                        {
                            throw new NotImplementedException();
                        }

                        throw new NotSupportedException();
                    }
                    break;

                default:
                    throw new NotSupportedException();
            }

            throw new NotSupportedException();
        }

        // todo: call this from the object pool
        public void Reset()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
        }
    }
}