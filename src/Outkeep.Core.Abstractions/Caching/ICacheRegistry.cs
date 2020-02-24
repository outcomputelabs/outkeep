using System.Threading;
using System.Threading.Tasks;

namespace Outkeep.Caching
{
    public interface ICacheRegistry
    {
        Task RegisterAsync(string key, int size, CancellationToken cancellationToken = default);

        Task UnregisterAsync(string key, CancellationToken cancellationToken = default);
    }
}