using Orleans.Runtime;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Outkeep.Registry
{
    public interface IRegistryGrainStorage
    {
        Task WriteStateAsync(string grainType, GrainReference grainReference, IKeyedGrainState state, CancellationToken cancellationToken = default);

        Task ReadStateAsync(string grainType, GrainReference grainReference, IKeyedGrainState state, CancellationToken cancellationToken = default);

        Task ClearStateAsync(string grainType, GrainReference grainReference, IKeyedGrainState state, CancellationToken cancellationToken = default);

        /// <summary>
        /// Provides a way to query the underlying storage by arbitrary criteria.
        /// Each specific storage provider is responsible for implementing its own <see cref="IQueryable{IKeyedGrainState}"/> provider.
        /// The specific <see cref="IQueryable"/> provider should at the very minimum support efficient filtering by key.
        /// It must also implement the <see cref="IAsyncEnumerable{IKeyedGrainState}"/> to allow async execution of the query.
        /// </summary>
        IQueryable<IKeyedGrainState> CreateQuery(string grainType, GrainReference grainReference);
    }
}