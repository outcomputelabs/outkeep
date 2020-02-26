using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;

namespace Outkeep.Registry
{
    internal class RegistryQuery<TState> : IQueryable<IRegistryEntryState<TState>>, IAsyncEnumerable<IRegistryEntryState<TState>>
        where TState : new()
    {
        private readonly RegistryQueryProvider<TState> _provider;

        public RegistryQuery(RegistryQueryProvider<TState> provider)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            Expression = Expression.Constant(this);
        }

        public RegistryQuery(RegistryQueryProvider<TState> provider, Expression expression)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));

            if (!typeof(IQueryable<IRegistryEntryState<TState>>).IsAssignableFrom(expression.Type))
                throw new ArgumentOutOfRangeException(nameof(expression));
        }

        public Type ElementType => typeof(IRegistryEntryState<TState>);

        public Expression Expression { get; }

        public IQueryProvider Provider => _provider;

        public IAsyncEnumerator<IRegistryEntryState<TState>> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<IRegistryEntryState<TState>> GetEnumerator()
        {
            return _provider.Execute<IEnumerable<IRegistryEntryState<TState>>>(Expression).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _provider.Execute<IEnumerable<IRegistryEntryState<TState>>>(Expression).GetEnumerator();
        }

        public override string ToString()
        {
            return _provider.GetQueryText(Expression);
        }
    }
}