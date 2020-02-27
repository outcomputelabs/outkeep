using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Outkeep.Registry
{
    internal class RegistryQueryTranslator : ExpressionVisitor
    {
        private readonly StringBuilder _builder = new StringBuilder();

        public string Translate(Expression expression)
        {
            _builder.Clear();

            Visit(expression);

            return _builder.ToString();
        }

        private static Expression StripQuotes(Expression expression)
        {
            while (expression.NodeType == ExpressionType.Quote)
            {
                expression = ((UnaryExpression)expression).Operand;
            }

            return expression;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.DeclaringType == typeof(IQueryable) && node.Method.Name == nameof(Queryable.Where))
            {
                _builder.Append("SELECT * FROM (");

                Visit(node.Arguments[0]);

                _builder.Append(") AS T WHERE ");

                var lambda = (LambdaExpression)StripQuotes(node.Arguments[1]);

                Visit(lambda.Body);

                return node;
            }

            throw new NotSupportedException($"Method {node.Method.Name} is not supported");
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            switch (node.NodeType)
            {
                case ExpressionType.Not:
                    _builder.Append(" NOT ");
                    Visit(node.Operand);
                    break;

                default:
                    throw new NotSupportedException($"Unary operator {node.NodeType} is not supported");
            }

            return node;
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            _builder.Append("(");

            Visit(node.Left);

            switch (node.NodeType)
            {
                case ExpressionType.Add:
                    _builder.Append(" AND ");
                    break;

                case ExpressionType.Or:
                    _builder.Append(" OR ");
                    break;

                case ExpressionType.Equal:
                    _builder.Append(" = ");
                    break;

                case ExpressionType.NotEqual:
                    _builder.Append(" <> ");
                    break;

                case ExpressionType.LessThan:
                    _builder.Append(" < ");
                    break;

                case ExpressionType.LessThanOrEqual:
                    _builder.Append(" <= ");
                    break;

                case ExpressionType.GreaterThan:
                    _builder.Append(" > ");
                    break;

                case ExpressionType.GreaterThanOrEqual:
                    _builder.Append(" >= ");
                    break;

                default:
                    throw new NotSupportedException($"Binary operator {node.NodeType} is not supported");
            }

            Visit(node.Right);

            _builder.Append(")");

            return node;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (node.Value is IQueryable queryable)
            {
                _builder.Append("SELECT * FROM ");
                _builder.Append(queryable.ElementType.Name);
            }
            else if (node.Value is null)
            {
                _builder.Append("NULL");
            }
            else
            {
                switch (Type.GetTypeCode(node.Value.GetType()))
                {
                    case TypeCode.Boolean:
                        _builder.Append((bool)node.Value ? "1" : "0");
                        break;

                    case TypeCode.String:
                        _builder.Append("'");
                        _builder.Append(node.Value);
                        _builder.Append("'");
                        break;

                    case TypeCode.Object:
                        throw new NotSupportedException($"Constant '{node.Value}' is not supported");

                    default:
                        _builder.Append(node.Value);
                        break;
                }
            }

            return node;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Expression != null && node.Expression.NodeType == ExpressionType.Parameter)
            {
                _builder.Append(node.Member.Name);

                return node;
            }

            throw new NotSupportedException($"Member '{node.Member.Name}' is not supported");
        }
    }
}