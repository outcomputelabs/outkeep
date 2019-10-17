using Orleans;
using Outkeep.Interfaces;
using System.Threading.Tasks;

namespace Outkeep.Implementations
{
    public class PingGrain : Grain, IPingGrain
    {
        public Task PingAsync() => Task.CompletedTask;
    }
}