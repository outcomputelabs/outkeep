using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using OutkeepResources = Outkeep.Properties.Resources;

namespace System.Linq
{
    public static class OutkeepQueryableExtensions
    {
        public static Task<ImmutableList<T>> ToImmutableListAsync<T>(this IQueryable<T> queryable, CancellationToken cancellationToken = default)
        {
            if (queryable is null) throw new ArgumentNullException(nameof(queryable));

            return InnerToImmutableListAsync(queryable, cancellationToken);
        }

        private static async Task<ImmutableList<T>> InnerToImmutableListAsync<T>(this IQueryable<T> queryable, CancellationToken cancellationToken = default)
        {
            var builder = ImmutableList.CreateBuilder<T>();

            await foreach (var item in queryable.AsAsyncEnumerable().WithCancellation(cancellationToken))
            {
                builder.Add(item);
            }

            return builder.ToImmutable();
        }

        public static IAsyncEnumerable<T> AsAsyncEnumerable<T>(this IQueryable<T> queryable)
        {
            if (queryable is IAsyncEnumerable<T> enumerable)
            {
                return enumerable;
            }

            throw new InvalidOperationException(OutkeepResources.Exception_IQueryableMustImplementIAsyncEnumerable);
        }
    }
}