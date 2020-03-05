using Orleans.Storage;
using Outkeep.Properties;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Outkeep.Caching.Memory
{
    internal class MemoryCacheRegistryStorage : ICacheRegistryStorage
    {
        private const string UnknownETag = "(unknown)";

        private readonly ConcurrentDictionary<string, MemoryCacheRegistryEntryState> _dictionary = new ConcurrentDictionary<string, MemoryCacheRegistryEntryState>();

        public Task ClearStateAsync(ICacheRegistryEntryState state)
        {
            if (state is null) throw new ArgumentNullException(nameof(state));

            // check if there is anything stored
            if (_dictionary.TryGetValue(state.Key, out var stored))
            {
                // if there is something stored then check if it has the same etag
                if (state.ETag == stored.ETag)
                {
                    // if there is has the same etag then attempt to remove the stored entry
                    if (_dictionary.TryRemove(new KeyValuePair<string, MemoryCacheRegistryEntryState>(state.Key, stored)))
                    {
                        // we succeeded in removing the exact stored entry
                        return Task.CompletedTask;
                    }
                    else
                    {
                        // some other thread replaced the stored entry in-between checking etags
                        throw new InconsistentStateException(Resources.Exception_CurrentETag_X_DoesNotMatchStoredETag_X, stored.ETag, state.ETag);
                    }
                }
                else
                {
                    // the etags do not match
                    throw new InconsistentStateException(Resources.Exception_CurrentETag_X_DoesNotMatchStoredETag_X, stored.ETag, state.ETag);
                }
            }

            // there is nothing stored
            state.ETag = null;
            return Task.CompletedTask;
        }

        public IQueryable<ICacheRegistryEntryState> CreateQuery()
        {
            return _dictionary.Values.AsQueryable();
        }

        public Task ReadStateAsync(ICacheRegistryEntryState state)
        {
            if (state is null) throw new ArgumentNullException(nameof(state));

            if (_dictionary.TryGetValue(state.Key, out var stored))
            {
                state.Size = stored.Size;
                state.AbsoluteExpiration = stored.AbsoluteExpiration;
                state.SlidingExpiration = stored.SlidingExpiration;
                state.ETag = stored.ETag;
            }

            return Task.CompletedTask;
        }

        public Task WriteStateAsync(ICacheRegistryEntryState state)
        {
            if (state is null) throw new ArgumentNullException(nameof(state));

            // check if there is anything stored already
            if (_dictionary.TryGetValue(state.Key, out var stored))
            {
                // if there is something stored then check if the etags match
                if (stored.ETag == state.ETag)
                {
                    // if the etags match then attempt to replace the stored entry
                    var inserted = new MemoryCacheRegistryEntryState(state.Key)
                    {
                        Size = state.Size,
                        AbsoluteExpiration = state.AbsoluteExpiration,
                        SlidingExpiration = state.SlidingExpiration,
                        ETag = Guid.NewGuid().ToString()
                    };

                    if (_dictionary.TryUpdate(state.Key, inserted, stored))
                    {
                        // we succeded in updating the stored entry
                        state.ETag = inserted.ETag;
                        return Task.CompletedTask;
                    }
                    else
                    {
                        // some other thread replaced the stored entry in-between checking etags
                        throw new InconsistentStateException(Resources.Exception_CurrentETag_X_DoesNotMatchStoredETag_X, UnknownETag, state.ETag);
                    }
                }
                else
                {
                    // the etags do not match
                    throw new InconsistentStateException(Resources.Exception_CurrentETag_X_DoesNotMatchStoredETag_X, stored.ETag, state.ETag);
                }
            }
            else
            {
                // if there is nothing stored then attempt to add a new entry
                var inserted = new MemoryCacheRegistryEntryState(state.Key)
                {
                    Size = state.Size,
                    AbsoluteExpiration = state.AbsoluteExpiration,
                    SlidingExpiration = state.SlidingExpiration,
                    ETag = Guid.NewGuid().ToString()
                };

                if (_dictionary.TryAdd(state.Key, inserted))
                {
                    // we succeeded in adding a new entry
                    state.ETag = inserted.ETag;
                    return Task.CompletedTask;
                }
                else
                {
                    // some other thread added a new entry in-between checking and inserting
                    throw new InconsistentStateException(Resources.Exception_CurrentETag_X_DoesNotMatchStoredETag_X, UnknownETag, state.ETag);
                }
            }
        }

        private class MemoryCacheRegistryEntryState : ICacheRegistryEntryState
        {
            public MemoryCacheRegistryEntryState(string key)
            {
                Key = key;
            }

            public string Key { get; }
            public string? ETag { get; set; }
            public int? Size { get; set; }
            public DateTimeOffset? AbsoluteExpiration { get; set; }
            public TimeSpan? SlidingExpiration { get; set; }
        }
    }
}