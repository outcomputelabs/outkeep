using Orleans.Runtime;
using System.Threading.Tasks;

namespace Outkeep.Governance
{
    public interface IWeakActivationExtension : IGrainExtension
    {
        Task DeactivateOnIdleAsync();
    }
}