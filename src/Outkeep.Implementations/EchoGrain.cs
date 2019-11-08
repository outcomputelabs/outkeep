using Orleans;
using Orleans.Concurrency;
using Outkeep.Interfaces;
using System.Threading.Tasks;

namespace Outkeep.Implementations
{
    [StatelessWorker(1)]
    internal class EchoGrain : Grain, IEchoGrain
    {
        public Task<string> EchoAsync(string message) =>
            Task.FromResult(message);
    }
}