using Orleans;
using Orleans.Concurrency;
using Outkeep.Interfaces;
using System.Threading.Tasks;

namespace Outkeep.Grains
{
    [StatelessWorker(1)]
    internal class EchoGrain : Grain, IEchoGrain
    {
        public Task<string> EchoAsync(string message) =>
            Task.FromResult(message);
    }
}