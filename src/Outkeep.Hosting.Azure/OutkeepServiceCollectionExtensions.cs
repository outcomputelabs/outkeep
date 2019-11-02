using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Outkeep.Hosting.Azure
{
    public static class OutkeepServiceCollectionExtensions
    {
        public static IServiceCollection AddOutkeepAzureClustering(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            // todo: add the orleans azure provider here with outkeep settings
        }
    }
}
