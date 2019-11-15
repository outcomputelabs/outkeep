using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Outkeep.Api.Http
{
    internal class OrleansActivityMiddleware : IMiddleware
    {
        private readonly ILogger<OrleansActivityMiddleware> logger;

        public OrleansActivityMiddleware(ILogger<OrleansActivityMiddleware> logger)
        {
            this.logger = logger;
        }

        [SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "N/A")]
        public Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            RequestContext.ActivityId = Guid.NewGuid();
            RequestContext.PropagateActivityId = true;

            logger.LogOutkeepActivityStarting(RequestContext.ActivityId);

            return next.Invoke(context);
        }
    }
}