using System.Threading.Tasks;

namespace Outkeep.Governance
{
    public interface IResourceGovernor<in TState>
    {
        Task EnlistAsync(IGrainControlExtension subject, TState state);

        Task LeaveAsync(IGrainControlExtension subject);
    }
}