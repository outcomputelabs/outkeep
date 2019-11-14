using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;

namespace Outkeep.Api.Http
{
    public class ReplaceVersionParameterInPathDocumentFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            if (swaggerDoc == null) throw new ArgumentNullException(nameof(swaggerDoc));

            var paths = new OpenApiPaths();
            foreach (var path in swaggerDoc.Paths)
            {
                paths.Add(path.Key.Replace("{version}", swaggerDoc.Info.Version, StringComparison.Ordinal), path.Value);
            }
            swaggerDoc.Paths = paths;
        }
    }
}