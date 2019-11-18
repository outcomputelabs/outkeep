using System;
using System.Threading.Tasks;

namespace Outkeep.Core
{
    public interface ICacheStorage
    {
        Task<(byte[] Value, DateTimeOffset? AbsoluteExpiration, TimeSpan? SlidingExpiration)?> TryReadAsync(string key);

        Task WriteAsync(string key, byte[] value, DateTimeOffset? absoluteExpiration, TimeSpan? slidingExpiration);

        Task ClearAsync(string key);
    }
}