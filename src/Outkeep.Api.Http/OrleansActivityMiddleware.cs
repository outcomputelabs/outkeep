using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using Outkeep.Api.Http.Properties;
using System;
using System.Threading.Tasks;

namespace Outkeep.Api.Http
{
    internal class OrleansActivityMiddleware : IMiddleware
    {
        private readonly ILogger _logger;

        public OrleansActivityMiddleware(ILogger<OrleansActivityMiddleware> logger)
        {
            _logger = logger;
        }

        public Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            RequestContext.ActivityId = Guid.NewGuid();
            RequestContext.PropagateActivityId = true;

            Log.OutkeepActivityStarting(_logger, RequestContext.ActivityId);

            return next.Invoke(context);
        }

        private static class Log
        {
            private static readonly Action<ILogger, Guid, Exception?> OutkeepActivityStartingAction =
                LoggerMessage.Define<Guid>(
                    LogLevel.Information,
                    new EventId(0, nameof(OutkeepActivityStarting)),
                    Resources.StartingActivity_X);

            public static void OutkeepActivityStarting(ILogger logger, Guid activityId) =>
                OutkeepActivityStartingAction(logger, activityId, null);
        }
    }
}