using Orleans.Services;
using System.Threading.Tasks;

namespace Outkeep.Interfaces
{
    /// <summary>
    /// Coordinates capacity claims between cache grains within a single host.
    /// </summary>
    public interface ICacheDirectorGrainService : IGrainService
    {
        Task PingAsync();
    }
}