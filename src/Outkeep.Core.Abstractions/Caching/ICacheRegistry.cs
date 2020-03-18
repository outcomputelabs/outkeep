using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Outkeep.Caching
{
    public interface ICacheRegistry
    {
        Task<ICacheRegistryEntry> GetEntryAsync(string key);

        Task<ImmutableList<ICacheRegistryEntry>> GetAllEntriesAsync();

        Task<ImmutableList<ICacheRegistryEntry>> GetTopEntriesBySizeAsync(bool ascending = false, int? limit = null);
    }
}