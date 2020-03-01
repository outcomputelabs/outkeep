using Orleans.Storage;
using Outkeep.Properties;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Outkeep.Caching
{
    public class MemoryCacheRegistryStorage : ICacheRegistryStorage
    {
        private readonly ConcurrentDictionary<string, CacheRegistryEntry> _dictionary = new ConcurrentDictionary<string, CacheRegistryEntry>();

        public Task ClearAsync(CacheRegistryEntry entry)
        {
            if (entry is null) throw new ArgumentNullException(nameof(entry));

            if (_dictionary.TryGetValue(entry.Key, out var stored))
            {
                if (entry.ETag == stored.ETag)
                {
                    if (_dictionary.TryRemove(new KeyValuePair<string, CacheRegistryEntry>(stored.Key, stored)))
                    {
                        return Task.CompletedTask;
                    }
                    else
                    {
                        throw new InconsistentStateException(Resources.Exception_CurrentETag_X_DoesNotMatchStoredETag_X, stored.ETag, entry.ETag);
                    }
                }
                else
                {
                    throw new InconsistentStateException(Resources.Exception_CurrentETag_X_DoesNotMatchStoredETag_X, stored.ETag, entry.ETag);
                }
            }

            return Task.CompletedTask;
        }

        public IQueryable<CacheRegistryEntry> CreateQuery()
        {
            return _dictionary.Values.AsQueryable();
        }

        public Task<CacheRegistryEntry?> ReadAsync(string key)
        {
            if (key is null) throw new ArgumentNullException(nameof(key));

            if (_dictionary.TryGetValue(key, out var entry))
            {
                return Task.FromResult<CacheRegistryEntry?>(entry);
            }

            return Task.FromResult<CacheRegistryEntry?>(null);
        }

        public Task<CacheRegistryEntry> WriteAsync(CacheRegistryEntry entry)
        {
            if (entry is null) throw new ArgumentNullException(nameof(entry));

            if (_dictionary.TryGetValue(entry.Key, out var stored))
            {
                if (stored.ETag == entry.ETag)
                {
                    var inserted = WithNewETag(entry);

                    if (_dictionary.TryUpdate(entry.Key, inserted, stored))
                    {
                        return Task.FromResult(inserted);
                    }
                    else
                    {
                        throw new InconsistentStateException(Resources.Exception_CurrentETag_X_DoesNotMatchStoredETag_X, stored.ETag, entry.ETag);
                    }
                }
                else
                {
                    throw new InconsistentStateException(Resources.Exception_CurrentETag_X_DoesNotMatchStoredETag_X, stored.ETag, entry.ETag);
                }
            }
            else
            {
                var inserted = WithNewETag(entry);

                if (_dictionary.TryAdd(entry.Key, inserted))
                {
                    return Task.FromResult(inserted);
                }
                else
                {
                    throw new InconsistentStateException();
                }
            }
        }

        private static CacheRegistryEntry WithNewETag(CacheRegistryEntry entry)
        {
            return new CacheRegistryEntry(
                entry.Key,
                entry.Size,
                entry.AbsoluteExpiration,
                entry.SlidingExpiration,
                entry.Timestamp,
                Guid.NewGuid().ToString("D", CultureInfo.InvariantCulture));
        }
    }
}