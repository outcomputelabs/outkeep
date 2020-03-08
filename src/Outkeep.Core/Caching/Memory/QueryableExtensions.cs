using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Outkeep.Caching.Memory
{
    internal static class QueryableExtensions
    {
        public static IQueryable<T> WithStateFactory<T>(this IQueryable<T> queryable, Expression<Func<string, T>> factory) where T : ICacheRegistryEntryState
        {
            if (queryable is null) throw new ArgumentNullException(nameof(queryable));
            if (factory is null) throw new ArgumentNullException(nameof(factory));

            var method = typeof(QueryableExtensions).GetMethod(nameof(WithStateFactory), BindingFlags.Static | BindingFlags.Public);

            return queryable.Provider.CreateQuery<T>(Expression.Call(null, method, queryable.Expression, Expression.Quote(factory)));
        }
    }
}