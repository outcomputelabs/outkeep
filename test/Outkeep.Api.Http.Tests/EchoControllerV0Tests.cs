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
            // arrange
            var message = "Some Message";

            var factory = Mock.Of<IGrainFactory>();
            Mock.Get(factory)
                .Setup(x => x.GetGrain<IEchoGrain>(Guid.Empty, null).EchoAsync(message))
                .Returns(new ValueTask<string>(message));

            var controller = new EchoController(factory);
            RequestContext.ActivityId = Guid.NewGuid();

            // act
            var result = await controller.GetAsync(message).ConfigureAwait(false);

            // assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var value = Assert.IsType<Echo>(ok.Value);
            Assert.Equal(RequestContext.ActivityId, value.ActivityId);
            Assert.Equal(message, value.Message);
            Assert.Equal("0", value.Version);
        }
    }
}