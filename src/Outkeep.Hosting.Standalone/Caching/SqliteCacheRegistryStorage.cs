using Microsoft.EntityFrameworkCore;
using Orleans.Storage;
using Outkeep.Caching;
using Outkeep.Hosting.Standalone.Properties;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Outkeep.Hosting.Standalone.Caching
{
    internal class SqliteCacheRegistryStorage : ICacheRegistryStorage
    {
        private readonly SqliteCacheRegistryContextFactory _factory;

        public SqliteCacheRegistryStorage(SqliteCacheRegistryContextFactory factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        /// <summary>
        /// Caches the compiled entity-by-key query to avoid redudant work.
        /// </summary>
        private static readonly Func<SqliteCacheRegistryContext, string, Task<SqliteCacheRegistryEntity>> _queryEntryByKey =
            EF.CompileAsyncQuery((SqliteCacheRegistryContext context, string key) =>
                context.CacheRegistryEntities.Where(x => x.Key == key).SingleOrDefault());

        public Task ReadStateAsync(ICacheRegistryEntryState state)
        {
            if (state is null) throw new ArgumentNullException(nameof(state));

            return InnerReadStateAsync(state);
        }

        private async Task InnerReadStateAsync(ICacheRegistryEntryState state)
        {
            using var context = _factory.Create();

            var result = await _queryEntryByKey(context, state.Key).ConfigureAwait(false);

            if (result is null)
            {
                state.ETag = null;
            }
            else
            {
                state.Size = result.Size;
                state.AbsoluteExpiration = result.AbsoluteExpiration;
                state.SlidingExpiration = result.SlidingExpiration;
                state.ETag = result.ETag;
            }
        }

        public Task WriteStateAsync(ICacheRegistryEntryState state)
        {
            if (state is null) throw new ArgumentNullException(nameof(state));

            return InnerWriteStateAsync(state);
        }

        private async Task InnerWriteStateAsync(ICacheRegistryEntryState state)
        {
            using var context = _factory.Create();

            var entity = await _queryEntryByKey(context, state.Key).ConfigureAwait(false);

            if (entity is null)
            {
                entity = new SqliteCacheRegistryEntity();
                context.Entry(entity).State = EntityState.Added;
            }
            else
            {
                context.Entry(entity).State = EntityState.Modified;
            }

            entity.Size = state.Size;
            entity.AbsoluteExpiration = state.AbsoluteExpiration;
            entity.SlidingExpiration = state.SlidingExpiration;
            await context.SaveChangesAsync().ConfigureAwait(false);

            state.ETag = entity.ETag;
        }

        public Task ClearStateAsync(ICacheRegistryEntryState state)
        {
            if (state is null) throw new ArgumentNullException(nameof(state));

            return InnerClearStateAsync(state);
        }

        private async Task InnerClearStateAsync(ICacheRegistryEntryState state)
        {
            using var context = _factory.Create();

            // check if the entry exists
            var stored = await _queryEntryByKey(context, state.Key).ConfigureAwait(false);
            if (stored is null)
            {
                // the entry does not exist so there is nothing to do
                return;
            }

            // ensure the etags match attempting database work
            if (stored.ETag != state.ETag)
            {
                throw new InconsistentStateException(Resources.Exception_CannotDeleteEntryWithKey_X_BecauseETagsDoeNotMatch.Format(state.Key), stored.ETag, state.ETag);
            }

            // attempt to remove the entity from the database
            context.Remove(stored);
            await context.SaveChangesAsync().ConfigureAwait(false);

            // reflect the clear in the user state
            state.ETag = null;
        }

        public IQueryable<ICacheRegistryEntryState> CreateQuery()
        {
            return _factory.Create().CacheRegistryEntities;
        }
    }
}