using System.Linq;

namespace Outkeep.Registry
{
    public interface IRegistryState<TState> where TState : class, new()
    {
        IQueryable<IRegistryEntryState<TState>> CreateQuery();
    }
}