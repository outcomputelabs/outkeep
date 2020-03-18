using Orleans;
using Orleans.Storage;
using System;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Outkeep.Caching.Memory
{
    internal class MemoryCacheRegistry : ICacheRegistry
    {
        private readonly IGrainFactory _factory;

        public MemoryCacheRegistry(IGrainFactory factory)
        {
            _factory = factory;
        }

        public async Task<ICacheRegistryEntry> GetEntryAsync(string key)
        {
            var entity = await _factory
                .GetMemoryCacheRegistryGrain()
                .TryGetEntityAsync(key)
                .ConfigureAwait(false);

            return entity is null ? new RegistryEntry(key, this) : ConvertToEntry(entity);
        }

        public Task ClearStateAsync(RegistryEntry entry)
        {
            if (entry is null) throw new ArgumentNullException(nameof(entry));

            return InnerClearStateAsync(entry);
        }

        private async Task InnerClearStateAsync(RegistryEntry entry)
        {
            var entity = new RegistryEntity(entry.Key, entry.Size, entry.AbsoluteExpiration, entry.SlidingExpiration, entry.ETag);

            try
            {
                await _factory
                    .GetMemoryCacheRegistryGrain()
                    .RemoveEntityAsync(entity)
                    .ConfigureAwait(false);
            }
            catch (MemoryCacheRegistryInconsistentStateException exception)
            {
                throw new InconsistentStateException(exception.Message, exception.StoredEtag, exception.CurrentETag, exception);
            }
        }

        public Task ReadStateAsync(RegistryEntry entry)
        {
            if (entry is null) throw new ArgumentNullException(nameof(entry));

            return InnerReadStateAsync(entry);
        }

        private async Task InnerReadStateAsync(RegistryEntry entry)
        {
            var entity = await _factory
                .GetMemoryCacheRegistryGrain()
                .TryGetEntityAsync(entry.Key)
                .ConfigureAwait(false);

            TryApplyEntity(entry, entity);
        }

        public Task WriteStateAsync(RegistryEntry entry)
        {
            if (entry is null) throw new ArgumentNullException(nameof(entry));

            return InnerWriteStateAsync(entry);
        }

        private async Task InnerWriteStateAsync(RegistryEntry entry)
        {
            var entity = ConvertToEntity(entry);

            RegistryEntity result;
            try
            {
                result = await _factory
                    .GetMemoryCacheRegistryGrain()
                    .WriteEntityAsync(entity)
                    .ConfigureAwait(false);
            }
            catch (MemoryCacheRegistryInconsistentStateException exception)
            {
                throw new InconsistentStateException(exception.Message, exception.StoredEtag, exception.CurrentETag, exception);
            }

            entry.ETag = result.ETag;
        }

        public async Task<ImmutableList<ICacheRegistryEntry>> GetEntriesAsync()
        {
            var result = await _factory
                .GetMemoryCacheRegistryGrain()
                .GetEntitiesAsync()
                .ConfigureAwait(false);

            return ConvertToEntries(result);
        }

        public async Task<ImmutableList<ICacheRegistryEntry>> GetTopEntriesBySizeAsync(bool ascending = false, int? limit = null)
        {
            var result = await _factory
                .GetMemoryCacheRegistryGrain()
                .GetTopEntitiesBySizeAsync(ascending, limit)
                .ConfigureAwait(false);

            return ConvertToEntries(result);
        }

        public ImmutableList<ICacheRegistryEntry> ConvertToEntries(ImmutableList<RegistryEntity> result)
        {
            // quick path for empty result
            if (result.IsEmpty)
            {
                return ImmutableList<ICacheRegistryEntry>.Empty;
            }

            // regular path for non empty result
            return result.ToBuilder().ConvertAll<ICacheRegistryEntry>(entity => ConvertToEntry(entity));
        }

        private RegistryEntry ConvertToEntry(RegistryEntity entity)
        {
            return new RegistryEntry(entity.Key, this)
            {
                Size = entity.Size,
                AbsoluteExpiration = entity.AbsoluteExpiration,
                SlidingExpiration = entity.SlidingExpiration,
                ETag = entity.ETag
            };
        }

        private static RegistryEntity ConvertToEntity(RegistryEntry entry)
        {
            return new RegistryEntity(
                entry.Key,
                entry.Size,
                entry.AbsoluteExpiration,
                entry.SlidingExpiration,
                entry.ETag);
        }

        private static void TryApplyEntity(RegistryEntry target, RegistryEntity? source)
        {
            if (source is null) return;

            target.Size = source.Size;
            target.AbsoluteExpiration = source.AbsoluteExpiration;
            target.SlidingExpiration = source.SlidingExpiration;
            target.ETag = source.ETag;
        }
    }
}