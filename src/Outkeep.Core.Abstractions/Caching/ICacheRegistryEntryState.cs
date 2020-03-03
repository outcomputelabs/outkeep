using System;

namespace Outkeep.Caching
{
    public interface ICacheRegistryEntryState
    {
        string Key { get; }

        string? ETag { get; set; }

        int? Size { get; set; }

        DateTimeOffset? AbsoluteExpiration { get; set; }

        TimeSpan? SlidingExpiration { get; set; }
    }
}