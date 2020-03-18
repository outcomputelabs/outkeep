using Orleans;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Outkeep.Caching.Memory
{
    internal interface IMemoryCacheRegistryGrain : IGrainWithGuidKey
    {
        Task RemoveEntityAsync(RegistryEntity entity);

        Task<RegistryEntity> WriteEntityAsync(RegistryEntity entity);

        Task<RegistryEntity?> TryGetEntityAsync(string key);

        Task<ImmutableList<RegistryEntity>> GetAllEntitiesAsync();
    }
}