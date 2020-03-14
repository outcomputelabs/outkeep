using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Outkeep.Caching.Memory
{
    internal class RegistryQueryProvider : IQueryProvider
    {
        private IGrainFactory _factory;

        public RegistryQueryProvider(IGrainFactory factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        public IQueryable CreateQuery(Expression expression)
        {
            var type = TypeUtility.GetElementType(expression.Type);

            return (IQueryable)Activator.CreateInstance(typeof(RegistryQuery<>).MakeGenericType(type), this, expression);
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new RegistryQuery<TElement>(this, expression);
        }

        public object Execute(Expression expression)
        {
            return Execute<IEnumerable<object>>(expression);
        }

        public TResult Execute<TResult>(Expression expression)
        {
            // todo: do query translation and execution here
            // todo: support IEnumerable<object>
            // todo: support IEnumerable<TResult>
            // todo: support IAsyncEnumerable<TResult>

            using (var translator = new RegistryQueryTranslator())
            {
                var query = translator.Translate(expression);

                throw new NotSupportedException();
                // _factory.GetGrain<IMemoryCacheRegistryGrain>(Guid.Empty)
                  //  .QueryAsync(query)
            }
        }
    }
}