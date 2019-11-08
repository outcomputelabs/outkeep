using Orleans;
using System.Threading.Tasks;

namespace Outkeep.Interfaces
{
    public interface IEchoGrain : IGrainWithGuidKey
    {
        Task<string> EchoAsync(string message);
    }
}