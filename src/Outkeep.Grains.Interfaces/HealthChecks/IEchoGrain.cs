using Orleans;
using System.Threading.Tasks;

namespace Outkeep.HealthChecks
{
    /// <summary>
    /// Interface for the echo test grain.
    /// </summary>
    public interface IEchoGrain : IGrainWithGuidKey
    {
        /// <summary>
        /// Echoes the given message back.
        /// </summary>
        ValueTask<string> EchoAsync(string message);
    }
}