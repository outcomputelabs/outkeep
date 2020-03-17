using Outkeep.Caching.Memory.Expressions;
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
            if (_isWhere)
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
                                _criteria.Add(new BinaryGrainQueryExpression(nameof(ICacheRegistryEntry.Key), (string)constant.Value));
                                return node;
                            }

                            throw new NotSupportedException();
                        }
                        break;

                    default:
                        throw new NotSupportedException();
                }

                throw new NotSupportedException();
            }

            throw new NotSupportedException();
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