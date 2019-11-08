using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using System;
using System.Threading.Tasks;

namespace Outkeep.Api.Rest
{
    public class ActivityMiddleware : IMiddleware
    {
        private readonly ILogger<ActivityMiddleware> logger;

        public ActivityMiddleware(ILogger<ActivityMiddleware> logger)
        {
            this.logger = logger;
        }

        public Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            RequestContext.ActivityId = Guid.NewGuid();
            RequestContext.PropagateActivityId = true;

            logger.LogOutkeepActivityStarting(RequestContext.ActivityId);

            return Task.CompletedTask;
        }
    }
}