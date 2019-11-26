using Microsoft.AspNetCore.Mvc;
using Moq;
using Orleans;
using Orleans.Runtime;
using Outkeep.Api.Http.Controllers.V0;
using Outkeep.Api.Http.Models.V0;
using Outkeep.Interfaces;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Outkeep.Api.Http.Tests
{
    public class EchoControllerV0Tests
    {
        [Fact]
        public async Task Echoes()
        {
            var message = "Some Message";
            var factory = Mock.Of<IGrainFactory>(x => x.GetGrain<IEchoGrain>(Guid.Empty, null).EchoAsync(message) == Task.FromResult(message));
            var controller = new EchoController(factory);
            RequestContext.ActivityId = Guid.NewGuid();

            var result = await controller.GetAsync(message).ConfigureAwait(false);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var value = Assert.IsType<Echo>(ok.Value);
            Assert.Equal(RequestContext.ActivityId, value.ActivityId);
            Assert.Equal(message, value.Message);
            Assert.Equal("0", value.Version);
        }
    }
}