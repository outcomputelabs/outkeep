﻿using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Outkeep.Api.Http
{
    internal class RemoveVersionFromParametersOperationFilter : IOperationFilter
    {
        private readonly RestApiServerOptions options;

        public RemoveVersionFromParametersOperationFilter(IOptions<RestApiServerOptions> options)
        {
            this.options = options?.Value;
        }

        public void Apply(Operation operation, OperationFilterContext context)
        {
            foreach (var parameter in operation.Parameters)
            {
                if (parameter.Name == options.VersionParameterName)
                {
                    operation.Parameters.Remove(parameter);
                    return;
                }
            }
        }
    }
}