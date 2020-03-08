using Orleans;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Outkeep.Caching.Memory
{
    internal interface IMemoryCacheRegistryStorageGrain : IGrainWithGuidKey
    {
        Task RemoveEntityAsync(MemoryCacheRegistryEntity entity);

        Task<MemoryCacheRegistryEntity> WriteEntityAsync(MemoryCacheRegistryEntity entity);

        Task<MemoryCacheRegistryEntity?> TryGetEntityAsync(string key);

        Task<ImmutableList<MemoryCacheRegistryEntity>> QueryAsync(GrainQuery query);
    }
}