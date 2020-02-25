using System.Collections.Generic;
using System.Threading.Tasks;

namespace Outkeep.Registry
{
    public interface IRegistryGrainState<TEntry>
    {
        Task WriteAsync(string key, TEntry entry);

        Task ClearAsync(string key);

        Task<TEntry> ReadAsync(string key);

        IAsyncEnumerable<TEntry> EnumerateAsync();
    }
}