using Microsoft.Extensions.Options;

namespace Outkeep.Caching
{
    /// <inheritdoc cref="ICacheActivationContext" />
    internal class CacheActivationContext : ICacheActivationContext
    {
        public CacheActivationContext(IOptions<CacheOptions> options, ISystemClock clock)
        {
            Options = options?.Value!;
            Clock = clock;
        }

        public CacheOptions Options { get; }

        public ISystemClock Clock { get; }
    }
}