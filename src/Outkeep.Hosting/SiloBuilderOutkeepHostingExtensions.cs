using Microsoft.Extensions.DependencyInjection;
using Outkeep.Governance;
using System;

namespace Orleans.Hosting
{
    public static class SiloBuilderOutkeepHostingExtensions
    {
        /// <summary>
        /// Adds all Outkeep grains to this silo.
        /// </summary>
        public static ISiloBuilder AddOutkeep(this ISiloBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder

                // add the outkeep grains to this silo
                .ConfigureApplicationParts(manager => manager.AddApplicationPart(typeof(SiloBuilderOutkeepHostingExtensions).Assembly).WithReferences())

                // add the weak activation extension
                .AddGrainExtension<IWeakActivationExtension, GrainControlExtension>()

                // add the outkeep core services
                .ConfigureServices(services =>
                {
                    services.AddOutkeep();
                });
        }
    }
}