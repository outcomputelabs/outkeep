using System.Linq;
using System.Linq.Expressions;

namespace Outkeep.Registry
{
    public interface IRegistryQueryProvider<TState> : IQueryProvider
    {
        string GetQueryText(Expression expression);
    }
}