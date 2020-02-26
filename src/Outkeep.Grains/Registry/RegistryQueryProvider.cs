using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Outkeep.Registry
{
    public class RegistryQueryProvider<TState> : IQueryProvider
        where TState : new()
    {
        public IQueryable CreateQuery(Expression expression)
        {
            var elementType = TypeSystem.GetElementType(expression.Type);

            try
            {
                var genericType = typeof(RegistryQuery<>).MakeGenericType(elementType);
                var args = new object[] { this, expression };
                var query = Activator.CreateInstance(genericType, args);
                return (IQueryable)query;
            }
            catch (TargetInvocationException exception)
            {
                throw exception.InnerException;
            }
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            if (typeof(TElement) != typeof(IRegistryEntryState<TState>))
                throw new NotSupportedException();

            var query = new RegistryQuery<TState>(this, expression);

            return (IQueryable<TElement>)query;
        }

        public object Execute(Expression expression)
        {
            return InnerExecute<object>(expression);
        }

        public TResult Execute<TResult>(Expression expression)
        {
            return InnerExecute<TResult>(expression);
        }

        private TResult InnerExecute<TResult>(Expression expression)
        {
            throw new NotSupportedException();
        }

        public string GetQueryText(Expression expression)
        {
            throw new NotImplementedException();
        }
    }
}