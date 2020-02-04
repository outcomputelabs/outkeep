using System;

namespace Orleans.Hosting
{
    public static class OutkeepGrainsSiloBuilderExtensions
    {
        /// <summary>
        /// Adds all Outkeep grains to this silo.
        /// </summary>
        public static ISiloBuilder AddOutkeepGrains(this ISiloBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.ConfigureApplicationParts(manager => manager.AddApplicationPart(typeof(OutkeepGrainsSiloBuilderExtensions).Assembly).WithReferences());
        }
    }
}