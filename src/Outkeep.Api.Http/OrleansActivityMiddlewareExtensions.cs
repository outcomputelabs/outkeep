using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Outkeep.Api.Http
{
    /// <summary>
    /// Extension methods for adding an <see cref="OrleansActivityMiddleware"/> to an <see cref="IApplicationBuilder"/> pipeline.
    /// </summary>
    public static class OrleansActivityMiddlewareExtensions
    {
        /// <summary>
        /// Adds an <see cref="OrleansActivityMiddleware"/> to the <see cref="IApplicationBuilder"/> pipeline.
        /// </summary>
        public static IApplicationBuilder UseActivityMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<OrleansActivityMiddleware>();
        }

        /// <summary>
        /// Adds the <see cref="OrleansActivityMiddleware"/> as a factory middleware to the <see cref="IServiceCollection"/>.
        /// </summary>
        public static IServiceCollection AddActivityMiddleware(this IServiceCollection services)
        {
            return services.AddTransient<OrleansActivityMiddleware>();
        }

        /// <summary>
        /// Adds the <see cref="OrleansActivityMiddleware"/> as a factory middleware to the <see cref="IServiceCollection"/> if it has not been added yet.
        /// </summary>
        public static IServiceCollection TryAddActivityMiddleware(this IServiceCollection services)
        {
            services.TryAddTransient<OrleansActivityMiddleware>();
            return services;
        }
    }
}