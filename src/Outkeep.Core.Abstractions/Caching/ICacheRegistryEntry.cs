using System;
using System.Threading.Tasks;

namespace Outkeep.Caching
{
    public interface ICacheRegistryEntry
    {
        string Key { get; }

        string? ETag { get; }

        int? Size { get; set; }

        DateTimeOffset? AbsoluteExpiration { get; set; }

        TimeSpan? SlidingExpiration { get; set; }

        Task ReadStateAsync();

        Task WriteStateAsync();

        Task ClearStateAsync();
    }
}