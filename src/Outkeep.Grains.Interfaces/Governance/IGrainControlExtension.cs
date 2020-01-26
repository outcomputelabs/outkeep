using Orleans.Runtime;
using System.Threading.Tasks;

namespace Outkeep.Governance
{
    public interface IGrainControlExtension : IGrainExtension
    {
        Task DeactivateOnIdleAsync();
    }
}