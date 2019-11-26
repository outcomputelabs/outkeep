using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.IO;
using System.Reflection;

namespace Outkeep.Api.Http
{
    internal class SwaggerGenOptionsConfigurator : IConfigureOptions<SwaggerGenOptions>
    {
        private readonly IApiVersionDescriptionProvider provider;
        private readonly OutkeepHttpApiServerOptions options;

        public SwaggerGenOptionsConfigurator(IApiVersionDescriptionProvider provider, IOptions<OutkeepHttpApiServerOptions> options)
        {
            this.provider = provider;
            this.options = options?.Value;
        }

        public void Configure(SwaggerGenOptions options)
        {
            options.DescribeAllParametersInCamelCase();

            // add a swagger page for each api version
            foreach (var description in provider.ApiVersionDescriptions)
            {
                options.SwaggerDoc(description.GroupName, new OpenApiInfo
                {
                    Title = this.options.Title,
                    Version = description.ApiVersion.ToString()
                });
            }

            // remove the version number as a parameter from swagger
            options.OperationFilter<RemoveVersionFromParametersOperationFilter>();

            // show the version number in the endpoint description
            options.DocumentFilter<ReplaceVersionParameterInPathDocumentFilter>();

            // add assembly comments to swagger
            options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml"));

            // allow swagger annotations in the controllers
            options.EnableAnnotations();
        }
    }
}