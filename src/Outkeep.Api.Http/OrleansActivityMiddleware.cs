using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using System;
using System.Threading.Tasks;

namespace Outkeep.Api.Http
{
    public class OrleansActivityMiddleware : IMiddleware
    {
        private readonly ILogger<OrleansActivityMiddleware> logger;

        public OrleansActivityMiddleware(ILogger<OrleansActivityMiddleware> logger)
        {
            this.logger = logger;

            InvokeAsync(null, null).Wait();
        }

        public Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            RequestContext.ActivityId = Guid.NewGuid();
            RequestContext.PropagateActivityId = true;

            logger.LogOutkeepActivityStarting(RequestContext.ActivityId);

            return next?.Invoke(context);
        }
    }
}