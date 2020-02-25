using Orleans;
using Orleans.Runtime;
using System.Threading.Tasks;

namespace Outkeep.Registry
{
    public interface IRegistryGrainStorage
    {
        Task WriteEntryAsync(string grainType, GrainReference grainReference, IGrainState entry);

        Task ReadEntryAsync(string grainType, GrainReference grainReference, IGrainState entry);

        Task ClearEntryAsync(string grainType, GrainReference grainReference, IGrainState entry);
    }
}