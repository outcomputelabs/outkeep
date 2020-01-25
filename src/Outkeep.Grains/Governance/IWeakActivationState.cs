using System.Threading.Tasks;

namespace Outkeep.Grains.Governance
{
    /// <summary>
    /// Lets grains inform an associated resource governor about their activity state to allow prioritized deactivation upon low resources.
    /// The resource governor chooses what information to take into account when deciding which grains to deactivate.
    /// </summary>
    public interface IWeakActivationState<out TState>
    {
        TState State { get; }

        Task ApplyAsync();
    }
}