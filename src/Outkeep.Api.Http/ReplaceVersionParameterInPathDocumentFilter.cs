using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Linq;

namespace Outkeep.Api.Http
{
    public class ReplaceVersionParameterInPathDocumentFilter : IDocumentFilter
    {
        private readonly RestApiServerOptions options;

        public ReplaceVersionParameterInPathDocumentFilter(IOptions<RestApiServerOptions> options)
        {
            this.options = options?.Value;
        }

        public void Apply(SwaggerDocument swaggerDoc, DocumentFilterContext context)
        {
            if (swaggerDoc == null) throw new ArgumentNullException(nameof(swaggerDoc));

            var token = $"{{{ options.VersionParameterName }}}";

            swaggerDoc.Paths = swaggerDoc.Paths.ToDictionary(
                path => path.Key.Replace(token, swaggerDoc.Info.Version, StringComparison.Ordinal),
                path => path.Value);
        }
    }
}