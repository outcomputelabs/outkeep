using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Outkeep.Api.Http
{
    public static class ActivityMiddlewareExtensions
    {
        public static IApplicationBuilder UseActivityMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ActivityMiddleware>();
        }

        public static IServiceCollection AddActivityMiddleware(this IServiceCollection services)
        {
            return services.AddTransient<ActivityMiddleware>();
        }
    }
}