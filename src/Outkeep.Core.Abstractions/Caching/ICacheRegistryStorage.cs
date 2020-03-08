using System.Linq;
using System.Threading.Tasks;

namespace Outkeep.Caching
{
    public interface ICacheRegistryStorage
    {
        Task ReadStateAsync(ICacheRegistryEntryState state);

        Task WriteStateAsync(ICacheRegistryEntryState state);

        Task ClearStateAsync(ICacheRegistryEntryState state);

        IQueryable<T> CreateQuery<T>(CreateStateAction<T> factory) where T : ICacheRegistryEntryState;
    }
}