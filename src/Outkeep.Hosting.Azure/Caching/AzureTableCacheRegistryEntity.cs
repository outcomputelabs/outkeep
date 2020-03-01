using Microsoft.Azure.Cosmos.Table;
using System;

namespace Outkeep.Caching
{
    public class AzureTableCacheRegistryEntity : TableEntity
    {
        public int? Size { get; set; }

        public DateTimeOffset? AbsoluteExpiration { get; set; }

        public TimeSpan? SlidingExpiration { get; set; }

        public static AzureTableCacheRegistryEntity FromEntry(string partitionKey, CacheRegistryEntry entry)
        {
            if (partitionKey is null) throw new ArgumentNullException(nameof(partitionKey));
            if (entry is null) throw new ArgumentNullException(nameof(entry));

            return new AzureTableCacheRegistryEntity
            {
                PartitionKey = partitionKey,
                RowKey = entry.Key,
                Size = entry.Size,
                AbsoluteExpiration = entry.AbsoluteExpiration,
                SlidingExpiration = entry.SlidingExpiration,
                Timestamp = entry.Timestamp,
                ETag = entry.ETag
            };
        }

        public CacheRegistryEntry ToEntry() => new CacheRegistryEntry(RowKey, Size, AbsoluteExpiration, SlidingExpiration, Timestamp, ETag);
    }
}