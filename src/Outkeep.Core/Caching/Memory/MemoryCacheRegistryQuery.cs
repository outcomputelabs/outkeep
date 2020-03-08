using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;

namespace Outkeep.Caching.Memory
{
    internal class MemoryCacheRegistryQuery<T> : IQueryable<T>, IAsyncEnumerable<T>
        where T : ICacheRegistryEntryState
    {
        public MemoryCacheRegistryQuery(RegistryQueryProvider provider)
        {
            Provider = provider ?? throw new ArgumentNullException(nameof(provider));
            Expression = Expression.Constant(this);
        }

        public MemoryCacheRegistryQuery(RegistryQueryProvider provider, Expression expression)
        {
            Provider = provider ?? throw new ArgumentNullException(nameof(provider));
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
        }

        public Type ElementType => typeof(T);

        public Expression Expression { get; }

        public IQueryProvider Provider { get; }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return Provider.Execute<IAsyncEnumerator<T>>(Expression);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Provider.Execute<IEnumerator<T>>(Expression);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Provider.Execute<IEnumerator>(Expression);
        }
    }
}