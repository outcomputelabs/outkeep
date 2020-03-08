using Orleans;
using Orleans.Storage;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Outkeep.Caching.Memory
{
    internal class MemoryCacheRegistryStorage : ICacheRegistryStorage
    {
        private readonly IGrainFactory _factory;
        private readonly RegistryQueryProvider _provider;

        public MemoryCacheRegistryStorage(IGrainFactory factory, RegistryQueryProvider provider)
        {
            _factory = factory;
            _provider = provider;
        }

        public Task ClearStateAsync(ICacheRegistryEntryState state)
        {
            if (state is null) throw new ArgumentNullException(nameof(state));

            return InnerClearStateAsync(state);
        }

        private async Task InnerClearStateAsync(ICacheRegistryEntryState state)
        {
            var entity = new MemoryCacheRegistryEntity(state.Key, state.Size, state.AbsoluteExpiration, state.SlidingExpiration, state.ETag);

            try
            {
                await _factory
                    .GetGrain<IMemoryCacheRegistryStorageGrain>(Guid.Empty)
                    .RemoveEntityAsync(entity)
                    .ConfigureAwait(false);
            }
            catch (MemoryCacheRegistryInconsistentStateException exception)
            {
                throw new InconsistentStateException(exception.Message, exception.StoredEtag, exception.CurrentETag, exception);
            }
        }

        public IQueryable<T> CreateQuery<T>(Func<string, T> factory) where T : ICacheRegistryEntryState
        {
            var query = new MemoryCacheRegistryQuery<T>(_provider)
                .Where(x => true)
                .WithStateFactory<T>(key => new MemoryCacheRegistryEntryState(key) as ICacheRegistryEntryState);

            return query;
        }

        public Task ReadStateAsync(ICacheRegistryEntryState state)
        {
            if (state is null) throw new ArgumentNullException(nameof(state));

            return InnerReadStateAsync(state);
        }

        private async Task InnerReadStateAsync(ICacheRegistryEntryState state)
        {
            var result = await _factory
                .GetGrain<IMemoryCacheRegistryStorageGrain>(Guid.Empty)
                .TryGetEntityAsync(state.Key)
                .ConfigureAwait(false);

            if (result != null)
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
            var entity = new MemoryCacheRegistryEntity(
                state.Key,
                state.Size,
                state.AbsoluteExpiration,
                state.SlidingExpiration,
                state.ETag);

            MemoryCacheRegistryEntity result;
            try
            {
                result = await _factory
                    .GetGrain<IMemoryCacheRegistryStorageGrain>(Guid.Empty)
                    .WriteEntityAsync(entity)
                    .ConfigureAwait(false);
            }
            catch (MemoryCacheRegistryInconsistentStateException exception)
            {
                throw new InconsistentStateException(exception.Message, exception.StoredEtag, exception.CurrentETag, exception);
            }

            state.ETag = result.ETag;
        }
    }
}