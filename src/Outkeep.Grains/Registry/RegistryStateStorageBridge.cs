using Orleans.Runtime;
using System;
using System.Threading.Tasks;

namespace Outkeep.Registry
{
    /// <summary>
    /// Acts as reusable bridge to save registry entities to storage.
    /// All the entities generated from a registry state object will share the same storage bridge.
    /// This reduces the memory footprint of each individual registry entry/entity state object generated from a shared parent registry state object.
    /// </summary>
    internal class RegistryStateStorageBridge<TState> where TState : class, new()
    {
        private readonly IRegistryGrainStorage<TState> _storage;
        private readonly string _fullStateName;
        private readonly GrainReference _grainReference;

        public RegistryStateStorageBridge(IRegistryGrainStorage<TState> storage, string fullStateName, GrainReference grainReference)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _fullStateName = fullStateName ?? throw new ArgumentNullException(nameof(fullStateName));
            _grainReference = grainReference ?? throw new ArgumentNullException(nameof(grainReference));
        }

        public Task ReadStateAsync(IRegistryEntity<TState> entity)
        {
            return _storage.ReadStateAsync(_fullStateName, _grainReference, entity);
        }

        public Task WriteStateAsync(IRegistryEntity<TState> entity)
        {
            return _storage.WriteStateAsync(_fullStateName, _grainReference, entity);
        }

        public Task ClearStateAsync(IRegistryEntity<TState> entity)
        {
            return _storage.ClearStateAsync(_fullStateName, _grainReference, entity);
        }
    }
}