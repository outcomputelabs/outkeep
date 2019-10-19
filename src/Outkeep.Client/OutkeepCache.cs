using Microsoft.Extensions.Caching.Distributed;
using Outkeep.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace Outkeep.Client
{
    public class OutkeepCache : IOutkeepCache
    {
        public OutkeepCache(IOutkeepClient client)
        {
            this.client = client;
        }

        private readonly IOutkeepClient client;

        public byte[] Get(string key)
        {
            return GetAsync(key).Result;
        }

        public async Task<byte[]> GetAsync(string key, CancellationToken token = default)
        {
            var result = await client.GetCacheGrain(key).GetAsync().ConfigureAwait(false);
            return result.Value;
        }

        public void Refresh(string key)
        {
            throw new System.NotImplementedException();
        }

        public Task RefreshAsync(string key, CancellationToken token = default)
        {
            throw new System.NotImplementedException();
        }

        public void Remove(string key)
        {
            RemoveAsync(key).Wait();
        }

        public Task RemoveAsync(string key, CancellationToken token = default)
        {
            return client.GetCacheGrain(key).RemoveAsync();
        }

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            throw new System.NotImplementedException();
        }

        public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            throw new System.NotImplementedException();
        }
    }
}