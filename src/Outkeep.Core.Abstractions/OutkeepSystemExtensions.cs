using System.Threading.Tasks;

namespace Outkeep
{
    /// <inheritdoc cref="Task.FromResult{TResult}(TResult)"/>
    public static class OutkeepSystemExtensions
    {
        public static Task<T> ToTask<T>(this T result)
        {
            return Task.FromResult(result);
        }
    }
}