using System.Threading.Tasks;

namespace Outkeep.Grains
{
    /// <summary>
    /// Lets grains inform an associated resource governor about their activity.
    /// The resource governor chooses what information to take into account when deciding which grains to deactivate upon low available resources.
    /// </summary>
    public interface IActivityState
    {
        /// <summary>
        /// Whether the grain is busy performing a long running task.
        /// </summary>
        public bool IsBusy { get; set; }

        /// <summary>
        /// The relative priority of the activity.
        /// Grains with <see cref="ActivityPriority.Critical"/> are never deactivated
        /// </summary>
        public ActivityPriority Priority { get; set; }

        /// <summary>
        /// An application-defined size for this activity.
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// Communicates the state to the resource governor.
        /// Whether the state is persisted or not depends on how the associated resource governor is configured.
        /// </summary>
        /// <exception cref="ResourceGovernorException">The resource claim was denied.</exception>
        public Task WriteStateAsync();
    }
}