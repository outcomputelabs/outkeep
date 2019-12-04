using System.Threading;
using System.Threading.Tasks;

namespace Outkeep.Core
{
    public interface ICacheStorage
    {
        Task<CacheItem?> ReadAsync(string key, CancellationToken cancellationToken = default);

        Task ClearAsync(string key, CancellationToken cancellationToken = default);

        Task WriteAsync(string key, CacheItem item, CancellationToken cancellationToken = default);
    }
}