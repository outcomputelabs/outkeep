using Orleans.Core;

namespace Outkeep.Registry
{
    public interface IRegistryEntryState<TState> : IStorage<TState> where TState : new()
    {
        public string Key { get; }
    }
}