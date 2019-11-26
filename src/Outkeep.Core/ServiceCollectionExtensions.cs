using Microsoft.Extensions.Hosting;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Utility extensions for <see cref="IServiceCollection"/> instances.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds <typeparamref name="T"/> as an <see cref="IHostedService"/> to the <see cref="IServiceCollection"/> if it is not yet present.
        /// </summary>
        public static void TryAddHostedService<T>(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            var service = new ServiceDescriptor(typeof(IHostedService), typeof(T), ServiceLifetime.Singleton);
            if (services.Contains(service)) return;
            services.Add(service);
        }
    }
}