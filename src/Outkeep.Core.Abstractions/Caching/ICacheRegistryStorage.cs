using System.Linq;
using System.Threading.Tasks;

namespace Outkeep.Caching
{
    public interface ICacheRegistryStorage
    {
        public Task<CacheRegistryEntry?> ReadAsync(string key);

        public Task ClearAsync(CacheRegistryEntry entry);

        public Task<CacheRegistryEntry> WriteAsync(CacheRegistryEntry entry);

        public IQueryable<CacheRegistryEntry> CreateQuery();
    }
}