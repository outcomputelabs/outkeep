using Outkeep.Properties;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Outkeep.Caching.Memory
{
    internal class RegistryQueryProvider : IQueryProvider
    {
        private IMemoryCacheRegistryStorageGrain _grain;

        public RegistryQueryProvider(IMemoryCacheRegistryStorageGrain grain)
        {
            _grain = grain ?? throw new ArgumentNullException(nameof(grain));
        }

        public IQueryable CreateQuery(Expression expression)
        {
            var type = TypeUtility.GetElementType(expression.Type);

            return (IQueryable)Activator.CreateInstance(typeof(MemoryCacheRegistryQuery<>).MakeGenericType(type), this, expression);
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new MemoryCacheRegistryQuery<TElement>(this, expression);
        }

        public object Execute(Expression expression)
        {
            return Execute<IEnumerable<object>>(expression);
        }

        public TResult Execute<TResult>(Expression expression)
        {
            throw new NotImplementedException();

            // todo: do query translation and execution here
            // todo: support IEnumerable<object>
            // todo: support IEnumerable<TResult>
            // todo: support IAsyncEnumerable<TResult>

            using (var translator = new RegistryQueryTranslator())
            {
                var query = translator.Translate(expression);
            }
        }

        private class AsyncEnumerator : IAsyncEnumerator<ICacheRegistryEntryState>
        {
            private readonly IMemoryCacheRegistryStorageGrain _grain;
            private readonly GrainQuery _query;

            public AsyncEnumerator(IMemoryCacheRegistryStorageGrain grain, GrainQuery query)
            {
                _grain = grain ?? throw new ArgumentNullException(nameof(grain));
                _query = query ?? throw new ArgumentNullException(nameof(query));
            }

            private bool _loaded = false;
            private ImmutableList<MemoryCacheRegistryEntity>.Enumerator? _enumerator = null;

            public ICacheRegistryEntryState Current
            {
                get
                {
                    if (_enumerator.HasValue)
                    {
                        return _enumerator.Value.Current;
                    }
                }
            }

            public ValueTask DisposeAsync()
            {
                throw new NotImplementedException();
            }

            public async ValueTask<bool> MoveNextAsync()
            {
                if (_enumerator is null)
                {
                    var results = await _grain.QueryAsync(_query).ConfigureAwait(false) ?? throw new InvalidOperationException(Resources.Exception_NullResultUnexpected);
                    _enumerator = results.GetEnumerator();
                }

                return _enumerator.Value.MoveNext();
            }
        }
    }
}