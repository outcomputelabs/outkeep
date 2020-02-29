using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;

namespace Outkeep.Registry
{
    [SuppressMessage("Naming", "CA1710:Identifiers should have correct suffix")]
    public interface IRegistryQuery<TState> : IQueryable<IKeyedGrainState<TState>> where TState : class
    {
        IRegistryQuery<TState> Where(Func<Expression<TState>, bool> predicate);
    }
}