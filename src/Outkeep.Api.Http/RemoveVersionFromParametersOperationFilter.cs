using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Outkeep.Api.Http
{
    internal class RemoveVersionFromParametersOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            foreach (var parameter in operation.Parameters)
            {
                if (parameter.Name == "version")
                {
                    operation.Parameters.Remove(parameter);
                    return;
                }
            }
        }
    }
}