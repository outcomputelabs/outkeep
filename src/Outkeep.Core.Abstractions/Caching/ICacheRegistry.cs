using System.Linq;
using System.Threading.Tasks;

namespace Outkeep.Caching
{
    public interface ICacheRegistry
    {
        Task<ICacheRegistryEntry> GetAsync(string key);

        IQueryable<ICacheRegistryEntry> CreateQuery();
    }
}