using Microsoft.Extensions.Caching.Distributed;

namespace Outkeep.Client
{
    public interface IOutkeepCache : IDistributedCache
    {
    }
}