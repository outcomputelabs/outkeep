using Orleans;
using System.Threading.Tasks;

namespace Outkeep.Interfaces
{
    public interface IPingGrain : IGrainWithGuidKey
    {
        Task PingAsync();
    }
}