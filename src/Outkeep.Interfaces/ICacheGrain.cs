using Orleans;
using Orleans.Concurrency;
using System;
using System.Threading.Tasks;

namespace Outkeep.Interfaces
{
    /// <summary>
    /// Interface for grains that provide basic distributed caching features.
    /// </summary>
    public interface ICacheGrain : IGrainWithStringKey
    {
        /// <summary>
        /// Gets the cached value.
        /// </summary>
        ValueTask<Immutable<byte[]?>> GetAsync();

        /// <summary>
        /// Clear the content from storage and release it from memory.
        /// </summary>
        Task RemoveAsync();

        /// <summary>
        /// Sets content and expiration options for a cache key.
        /// </summary>
        Task SetAsync(Immutable<byte[]?> value, DateTimeOffset? absoluteExpiration, TimeSpan? slidingExpiration);

        /// <summary>
        /// Touches the cached item without retrieving it as to delay the sliding expiration.
        /// </summary>
        Task RefreshAsync();

        /// <summary>
        /// Long polls the grain for changes to the cache item.
        /// </summary>
        Task<TaggedValue<Guid, byte[]?>> PollAsync(Guid etag);
    }
}