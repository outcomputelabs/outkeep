using Orleans.Services;
using System.Threading.Tasks;

namespace Outkeep.Interfaces
{
    public interface ICacheDirectorGrainService : IGrainService
    {
        Task PingAsync();
    }
}