using Microsoft.AspNetCore.Mvc;
using Moq;
using Orleans;
using Orleans.Concurrency;
using Outkeep.Api.Http.Controllers.V1;
using Outkeep.Interfaces;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Outkeep.Api.Http.Tests
{
    public class CacheControllerV1Tests
    {
        private static readonly byte[] absent = null;
        private static readonly byte[] present = Array.Empty<byte>();

        [Fact]
        public async Task GetNonExistingKeyReturnsNoContent()
        {
            var key = Guid.NewGuid().ToString();
            var factory = Mock.Of<IGrainFactory>(x => x.GetGrain<ICacheGrain>(key, null).GetAsync() == Task.FromResult(absent.AsImmutable()));
            var controller = new CacheController(factory);

            var result = await controller.GetAsync(key).ConfigureAwait(false);

            Assert.IsType<NoContentResult>(result.Result);
        }

        [Fact]
        public async Task GetExistingKeyReturnsOk()
        {
            var key = Guid.NewGuid().ToString();
            var factory = Mock.Of<IGrainFactory>(x => x.GetGrain<ICacheGrain>(key, null).GetAsync() == Task.FromResult(present.AsImmutable()));
            var controller = new CacheController(factory);

            var result = await controller.GetAsync(key).ConfigureAwait(false);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var content = Assert.IsType<byte[]>(ok.Value);
            Assert.Same(present, content);
        }
    }
}