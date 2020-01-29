using System.Threading.Tasks;

namespace Outkeep.Governance
{
    public interface IResourceGovernor
    {
        Task EnlistAsync(IWeakActivationExtension subject, IWeakActivationFactor factor);

        Task LeaveAsync(IWeakActivationExtension subject);
    }
}