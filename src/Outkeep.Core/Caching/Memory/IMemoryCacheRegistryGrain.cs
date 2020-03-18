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

        Task<ImmutableList<RegistryEntity>> GetTopEntitiesBySizeAsync(bool ascending = false, int? limit = null);
    }
}