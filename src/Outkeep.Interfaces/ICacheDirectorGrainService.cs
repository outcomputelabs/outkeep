using Orleans.Services;
using System.Threading.Tasks;

namespace Outkeep.Interfaces
{
    /// <summary>
    /// Supports the <see cref="ICacheDirector"/> by running maintenance tasks on the Orleans scheduler to reduce context switching.
    /// </summary>
    public interface ICacheDirectorGrainService : IGrainService
    {
        Task PingAsync();
    }
}