using Orleans;
using System.Threading.Tasks;

namespace Outkeep.Caching.Memory
{
    internal interface IMemoryCacheRegistryStorageGrain : IGrainWithGuidKey
    {
        Task RemoveEntityAsync(MemoryCacheRegistryEntity entity);

        Task<MemoryCacheRegistryEntity> WriteEntityAsync(MemoryCacheRegistryEntity entity);

        Task<MemoryCacheRegistryEntity?> TryGetEntityAsync(string key);
    }
}