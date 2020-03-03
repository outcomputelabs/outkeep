using Microsoft.Extensions.DependencyInjection;
using System;

namespace Outkeep.Hosting.Standalone.Caching
{
    internal class SqliteCacheRegistryContextFactory
    {
        private readonly IServiceProvider _provider;

        public SqliteCacheRegistryContextFactory(IServiceProvider provider)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        public SqliteCacheRegistryContext Create() => _provider.GetRequiredService<SqliteCacheRegistryContext>();
    }
}