using Orleans;
using Orleans.Concurrency;
using System;
using System.Threading.Tasks;

namespace Outkeep.Interfaces
{
    public interface ICacheGrain : IGrainWithStringKey
    {
        ValueTask<Immutable<byte[]>> GetAsync();

        Task RemoveAsync();

        Task SetAsync(Immutable<byte[]> value, DateTimeOffset? absoluteExpiration, TimeSpan? slidingExpiration);

        Task RefreshAsync();

        Task<ReactiveResult<byte[]>> PollAsync(Guid etag);
    }
}