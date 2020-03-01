using System.Linq;
using System.Threading.Tasks;

namespace Outkeep.Caching
{
    public interface ICacheRegistry
    {
        Task<CacheRegistryEntry> RegisterOrUpdateAsync(CacheRegistryEntry entry);

        Task<CacheRegistryEntry?> GetAsync(string key);

        Task UnregisterAsync(CacheRegistryEntry entry);

        IQueryable<CacheRegistryEntry> CreateQuery();
    }
}