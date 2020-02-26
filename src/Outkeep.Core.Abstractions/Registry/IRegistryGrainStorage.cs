using Orleans.Runtime;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Outkeep.Registry
{
    public interface IRegistryGrainStorage<TState> where TState : class
    {
        Task WriteStateAsync(string grainType, GrainReference grainReference, IRegistryEntity<TState> state);

        Task ReadStateAsync(string grainType, GrainReference grainReference, IRegistryEntity<TState> state);

        Task ClearStateAsync(string grainType, GrainReference grainReference, IRegistryEntity<TState> state);

        /// <summary>
        /// Provides a way to query the underlying storage by arbitrary predicates while allowing conversion to the user model.
        /// Each specific storage provider is responsible for implementing its own <see cref="IQueryable"/> provider.
        /// The specific <see cref="IQueryable"/> provider should at the very minimum support efficient filtering by key.
        /// It must also implement the <see cref="System.Collections.Generic.IAsyncEnumerable{T}"/> to allow async execution of the query.
        /// </summary>
        IQueryable<IRegistryEntity<TState>> CreateQuery<TOutput>(string grainType, GrainReference grainReference, Func<IRegistryEntity<TState>, TOutput> factory);
    }
}