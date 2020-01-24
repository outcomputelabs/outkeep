using Orleans.Runtime;
using System.Threading.Tasks;

namespace Outkeep.Grains
{
    public interface IGrainControlExtension : IGrainExtension
    {
        Task DeactivateOnIdleAsync();
    }
}