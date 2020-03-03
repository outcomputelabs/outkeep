using Outkeep.Caching;
using System;
using System.ComponentModel.DataAnnotations;

namespace Outkeep.Hosting.Standalone.Caching
{
    internal class SqliteCacheRegistryEntity : ICacheRegistryEntryState
    {
        [Key]
        public string Key { get; set; } = null!;

        public int? Size { get; set; }

        public DateTimeOffset? AbsoluteExpiration { get; set; }

        public TimeSpan? SlidingExpiration { get; set; }

        [Timestamp]
        public string? ETag { get; set; }
    }
}