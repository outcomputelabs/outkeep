using Orleans.Runtime;
using System.Threading.Tasks;

namespace Outkeep.Governance
{
    public interface IResourceGovernor<in TState>
    {
        Task EnlistAsync(GrainReference subject, TState state);

        Task LeaveAsync(GrainReference subject);
    }
}