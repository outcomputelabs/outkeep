using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;

namespace Outkeep.Api.Http
{
    /// <summary>
    /// Replaces the version parameter in all paths with the specific version number for that path.
    /// </summary>
    public class ReplaceVersionParameterInPathDocumentFilter : IDocumentFilter
    {
        /// <summary>
        /// Replaces the version parameter in all paths with the specific version the user is browsing.
        /// </summary>
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