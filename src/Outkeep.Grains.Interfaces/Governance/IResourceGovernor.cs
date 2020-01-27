using System.Threading.Tasks;

namespace Outkeep.Governance
{
    public interface IResourceGovernor<in TState>
    {
        Task EnlistAsync(IWeakActivationExtension subject, TState state);

        Task LeaveAsync(IWeakActivationExtension subject);
    }
}