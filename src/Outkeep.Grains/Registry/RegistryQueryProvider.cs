using Microsoft.Extensions.ObjectPool;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Outkeep.Registry
{
    internal class RegistryQueryProvider<TState> : IRegistryQueryProvider<TState>
        where TState : new()
    {
        public readonly ObjectPool<RegistryQueryTranslator> _translators;

        public RegistryQueryProvider(ObjectPool<RegistryQueryTranslator> translators)
        {
            _translators = translators ?? throw new ArgumentNullException(nameof(translators));
        }

        public IQueryable CreateQuery(Expression expression)
        {
            if (expression is null) throw new ArgumentNullException(nameof(expression));

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
            throw new NotImplementedException();
        }

        public string GetQueryText(Expression expression)
        {
            var translator = _translators.Get();
            try
            {
                return translator.Translate(expression);
            }
            finally
            {
                _translators.Return(translator);
            }
        }
    }
}