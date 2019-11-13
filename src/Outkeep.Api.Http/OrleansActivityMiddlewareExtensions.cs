using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Outkeep.Api.Http
{
    public static class OrleansActivityMiddlewareExtensions
    {
        public static IApplicationBuilder UseActivityMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<OrleansActivityMiddleware>();
        }

        public static IServiceCollection AddActivityMiddleware(this IServiceCollection services)
        {
            return services.AddTransient<OrleansActivityMiddleware>();
        }
    }
}