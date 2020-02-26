using System.Linq.Expressions;
using System.Text;

namespace Outkeep.Registry
{
    internal class RegistryQueryTranslator : ExpressionVisitor
    {
        public string Translate(Expression expression)
        {
            var builder = new StringBuilder();

            Visit(expression);

            return builder.ToString();
        }

        private static Expression StripQuotes(Expression expression)
        {
            while (expression.NodeType == ExpressionType.Quote)
            {
                expression = ((UnaryExpression)expression).Operand;
            }

            return expression;
        }
    }
}