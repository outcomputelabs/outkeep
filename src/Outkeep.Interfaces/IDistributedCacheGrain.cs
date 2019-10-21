using Orleans;
using Orleans.Concurrency;
using System;
using System.Threading.Tasks;

namespace Outkeep.Interfaces
{
    public interface IDistributedCacheGrain : IGrainWithStringKey
    {
        Task<Immutable<byte[]>> GetAsync();

        Task RemoveAsync();

        Task SetAsync(Immutable<byte[]> value, DateTimeOffset? absoluteExpiration, TimeSpan? slidingExpiration);

        Task RefreshAsync();
    }
}