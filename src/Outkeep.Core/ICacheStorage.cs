using System;
using System.Threading;
using System.Threading.Tasks;

namespace Outkeep.Core
{
    public interface ICacheStorage
    {
        Task<(byte[] Value, DateTimeOffset? AbsoluteExpiration, TimeSpan? SlidingExpiration)?> ReadAsync(string key, CancellationToken cancellationToken = default);

        Task ClearAsync(string key, CancellationToken cancellationToken = default);

        Task WriteAsync(string key, byte[] value, DateTimeOffset? absoluteExpiration, TimeSpan? slidingExpiration, CancellationToken cancellationToken = default);
    }
}