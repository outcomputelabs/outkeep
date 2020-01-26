using Orleans.Runtime;
using System.Threading.Tasks;

namespace Outkeep.Grains.Governance
{
    public interface IGrainControlExtension : IGrainExtension
    {
        Task DeactivateOnIdleAsync();
    }
}