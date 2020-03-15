using Orleans;
using Orleans.Storage;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Outkeep.Caching.Memory
{
    internal class MemoryCacheRegistry : ICacheRegistry
    {
        private readonly IGrainFactory _factory;
        private readonly RegistryQueryProvider _provider;

        public MemoryCacheRegistry(IGrainFactory factory)
        {
            _factory = factory;
            _provider = new RegistryQueryProvider(_factory, this);
        }

        public async Task<ICacheRegistryEntry> GetAsync(string key)
        {
            var entity = await _factory
                .GetMemoryCacheRegistryGrain()
                .TryGetEntityAsync(key)
                .ConfigureAwait(false);

            var entry = new RegistryEntry(key, this);
            if (entity != null)
            {
                entry.AbsoluteExpiration = entity.AbsoluteExpiration;
                entry.SlidingExpiration = entity.SlidingExpiration;
                entry.Size = entity.Size;
                entry.ETag = entity.ETag;
            }
            return entry;
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

            if (entity != null)
            {
                entry.Size = entity.Size;
                entry.AbsoluteExpiration = entity.AbsoluteExpiration;
                entry.SlidingExpiration = entity.SlidingExpiration;
                entry.ETag = entity.ETag;
            }
        }

        public Task WriteStateAsync(RegistryEntry entry)
        {
            if (entry is null) throw new ArgumentNullException(nameof(entry));

            return InnerWriteStateAsync(entry);
        }

        private async Task InnerWriteStateAsync(RegistryEntry entry)
        {
            var entity = new RegistryEntity(
                entry.Key,
                entry.Size,
                entry.AbsoluteExpiration,
                entry.SlidingExpiration,
                entry.ETag);

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

        public IQueryable<ICacheRegistryEntry> CreateQuery()
        {
            return _provider.CreateQuery<RegistryEntry>();
        }
    }
}