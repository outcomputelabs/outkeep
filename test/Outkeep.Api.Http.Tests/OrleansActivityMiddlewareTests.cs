using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Orleans.Runtime;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Outkeep.Api.Http.Tests
{
    public class OrleansActivityMiddlewareTests
    {
        [Fact]
        public async Task GeneratesActivity()
        {
            var logger = Mock.Of<ILogger<OrleansActivityMiddleware>>();
            var middleware = new OrleansActivityMiddleware(logger);

            var context = Mock.Of<HttpContext>();
            var next = Mock.Of<RequestDelegate>(x => x.Invoke(context) == Task.CompletedTask);
            await middleware.InvokeAsync(context, next).ConfigureAwait(false);

            Assert.NotEqual(Guid.Empty, RequestContext.ActivityId);
            Assert.True(RequestContext.PropagateActivityId);
            Mock.Get(next).VerifyAll();
        }
    }
}