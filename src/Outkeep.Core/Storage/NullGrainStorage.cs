using Orleans;
using Orleans.Runtime;
using Orleans.Storage;
using System.Threading.Tasks;

namespace Outkeep.Storage
{
    /// <summary>
    /// Implements an <see cref="IGrainStorage"/> service that does not write or read state.
    /// For use with testing or to disable persistence where applicable.
    /// </summary>
    internal sealed class NullGrainStorage : IGrainStorage
    {
        /// <inheritdoc />
        public Task ClearStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task ReadStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task WriteStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
        {
            return Task.CompletedTask;
        }
    }
}