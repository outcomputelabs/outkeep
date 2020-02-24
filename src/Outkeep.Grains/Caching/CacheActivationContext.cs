using Microsoft.Extensions.Options;

namespace Outkeep.Caching
{
    /// <inheritdoc cref="ICacheActivationContext" />
    internal class CacheActivationContext : ICacheActivationContext
    {
        public CacheActivationContext(IOptions<CacheOptions> options, ISystemClock clock, ICacheRegistry cacheRegistry)
        {
            Options = options?.Value!;
            Clock = clock;
            CacheRegistry = cacheRegistry;
        }

        public CacheOptions Options { get; }

        public ISystemClock Clock { get; }

        public ICacheRegistry CacheRegistry { get; }
    }
}