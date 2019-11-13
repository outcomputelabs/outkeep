using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.IO;
using System.Reflection;

namespace Outkeep.Api.Http
{
    internal class VersionedApiExplorerSwaggerOptionsConfigurator : IConfigureOptions<SwaggerGenOptions>
    {
        private readonly IApiVersionDescriptionProvider provider;
        private readonly OutkeepHttpApiServerOptions options;

        public VersionedApiExplorerSwaggerOptionsConfigurator(IApiVersionDescriptionProvider provider, IOptions<OutkeepHttpApiServerOptions> options)
        {
            this.provider = provider;
            this.options = options?.Value;
        }

        public void Configure(SwaggerGenOptions swagger)
        {
            // add a swagger page for each api version
            foreach (var description in provider.ApiVersionDescriptions)
            {
                swagger.SwaggerDoc(description.GroupName, new Info
                {
                    Title = options.Title,
                    Version = description.ApiVersion.ToString()
                });
            }

            // remove the version number as a parameter from swagger
            if (options.RemoveVersionFromParameters)
            {
                swagger.OperationFilter<RemoveVersionFromParametersOperationFilter>();
            }

            // show the version number in the endpoint description
            if (options.ReplaceVersionParameterInPath)
            {
                swagger.DocumentFilter<ReplaceVersionParameterInPathDocumentFilter>();
            }

            // add assembly comments to swagger
            swagger.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml"));

            // allow swagger annotations in the controllers
            swagger.EnableAnnotations();
        }
    }
}