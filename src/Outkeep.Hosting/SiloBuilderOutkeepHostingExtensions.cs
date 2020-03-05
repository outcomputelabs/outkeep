using Microsoft.Extensions.DependencyInjection;
using System;

namespace Orleans.Hosting
{
    public static class SiloBuilderOutkeepHostingExtensions
    {
        /// <summary>
        /// Adds all Outkeep artefacts to this silo.
        /// </summary>
        public static ISiloBuilder AddOutkeep(this ISiloBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder

                // add the outkeep grains to this silo
                .ConfigureApplicationParts(manager => manager.AddApplicationPart(typeof(SiloBuilderOutkeepHostingExtensions).Assembly).WithReferences())

                // add the outkeep silo extensions
                .AddWeakActivationExtension()

                // add the outkeep core services
                .ConfigureServices(services =>
                {
                    services.AddOutkeep();
                });
        }
    }
}