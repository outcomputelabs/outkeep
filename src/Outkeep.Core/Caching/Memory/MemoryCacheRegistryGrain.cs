using Orleans;
using Outkeep.Properties;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Outkeep.Caching.Memory
{
    /// <summary>
    /// This grain keeps the cache registry information in memory.
    /// This is for use with unit testing and one-box non-reliable deployments.
    /// </summary>
    internal class MemoryCacheRegistryGrain : Grain, IMemoryCacheRegistryGrain
    {
        private readonly Dictionary<string, RegistryEntity> _dictionary = new Dictionary<string, RegistryEntity>();

        public Task<RegistryEntity?> TryGetEntityAsync(string key)
        {
            if (_dictionary.TryGetValue(key, out var entity))
            {
                return Task.FromResult<RegistryEntity?>(entity);
            }

            return Task.FromResult<RegistryEntity?>(null);
        }

        public Task RemoveEntityAsync(RegistryEntity entity)
        {
            if (entity is null) throw new ArgumentNullException(nameof(entity));

            // check if there is anything stored
            if (_dictionary.TryGetValue(entity.Key, out var stored))
            {
                // if there is something stored then check if it has the same etag
                if (entity.ETag == stored.ETag)
                {
                    // if there is has the same etag then we can remove it
                    _dictionary.Remove(entity.Key);
                }
                else
                {
                    // the etags do not match
                    throw new MemoryCacheRegistryInconsistentStateException(Resources.Exception_CurrentETag_X_DoesNotMatchStoredETag_X, stored.ETag, entity.ETag);
                }
            }

            return Task.CompletedTask;
        }

        public Task<RegistryEntity> WriteEntityAsync(RegistryEntity entity)
        {
            // check if there is anything stored already
            if (_dictionary.TryGetValue(entity.Key, out var stored))
            {
                // if there is something stored then check if the etags match
                if (stored.ETag != entity.ETag)
                {
                    // the etags do not match
                    throw new MemoryCacheRegistryInconsistentStateException(Resources.Exception_CurrentETag_X_DoesNotMatchStoredETag_X, stored.ETag, entity.ETag);
                }
            }
            else
            {
                // if there is nothing stored then ensure the incoming etag is null
                if (entity.ETag != null)
                {
                    throw new MemoryCacheRegistryInconsistentStateException(Resources.Exception_CurrentETag_X_DoesNotMatchStoredETag_X, null, entity.ETag);
                }
            }

            // if all checks are okay then insert the entry while generating a new etag
            var inserted = new RegistryEntity(
                entity.Key,
                entity.Size,
                entity.AbsoluteExpiration,
                entity.SlidingExpiration,
                Guid.NewGuid().ToString("D", CultureInfo.InvariantCulture));

            _dictionary[entity.Key] = inserted;

            return Task.FromResult(inserted);
        }

        public Task<ImmutableList<RegistryEntity>> GetEntitiesAsync()
        {
            var builder = ImmutableList.CreateBuilder<RegistryEntity>();

            foreach (var item in _dictionary)
            {
                builder.Add(item.Value);
            }

            return Task.FromResult(builder.ToImmutable());
        }

        public Task<ImmutableList<RegistryEntity>> GetTopEntitiesBySizeAsync(bool ascending = false, int? limit = null)
        {
            var query = _dictionary.AsEnumerable();

            query = ascending
                ? query.OrderBy(x => x.Value.Size)
                : query.OrderByDescending(x => x.Value.Size);

            query = limit.HasValue
                ? query.Take(limit.Value)
                : query;

            return Task.FromResult(query.Select(x => x.Value).ToImmutableList());
        }
    }
}