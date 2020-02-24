using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Outkeep.Caching
{
    public interface ICacheRegistryStorage
    {
        Task WriteAsync(string key, int size, CancellationToken cancellationToken = default);

        Task ClearAsync(string key, CancellationToken cancellationToken = default);

        IAsyncEnumerable<CacheRegistryEntry> EnumerateAsync(CancellationToken cancellationToken = default);
    }
}