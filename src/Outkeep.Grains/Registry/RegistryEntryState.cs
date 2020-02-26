using System;
using System.Threading.Tasks;

namespace Outkeep.Registry
{
    /// <summary>
    /// Acts as a bridge state object between the user entity object and the storage entity object.
    /// </summary>
    internal class RegistryEntryState<TState> : IRegistryEntryState<TState>, IRegistryEntity<TState>
        where TState : class, new()
    {
        private readonly RegistryStateStorageBridge<TState> _bridge;

        public RegistryEntryState(RegistryStateStorageBridge<TState> bridge, string key, TState state, string etag)
        {
            _bridge = bridge ?? throw new ArgumentNullException(nameof(bridge));

            Key = key ?? throw new ArgumentNullException(nameof(key));
            State = state;
            Etag = etag;
        }

        public string Key { get; }

        public TState State { get; set; }

        public string Etag { get; set; }

        public Task ClearStateAsync()
        {
            return _bridge.ClearStateAsync(this);
        }

        public Task ReadStateAsync()
        {
            return _bridge.ReadStateAsync(this);
        }

        public Task WriteStateAsync()
        {
            return _bridge.WriteStateAsync(this);
        }
    }
}