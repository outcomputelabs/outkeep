using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Orleans;
using Orleans.Concurrency;
using Outkeep.Api.Http.Controllers.V1;
using Outkeep.Interfaces;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Outkeep.Api.Http.Tests
{
    public class CacheControllerV1Tests
    {
        [Fact]
        public async Task GettingNonExistingKeyReturnsNoContent()
        {
            var key = Guid.NewGuid().ToString();
            byte[] value = null;
            var factory = Mock.Of<IGrainFactory>(x => x.GetGrain<ICacheGrain>(key, null).GetAsync() == Task.FromResult(value.AsImmutable()));
            var controller = new CacheController(factory);

            var result = await controller.GetAsync(key).ConfigureAwait(false);

            Assert.IsType<NoContentResult>(result.Result);
        }

        [Fact]
        public async Task GettingExistingKeyReturnsOk()
        {
            var key = Guid.NewGuid().ToString();
            var value = Guid.NewGuid().ToByteArray();
            var factory = Mock.Of<IGrainFactory>(x => x.GetGrain<ICacheGrain>(key, null).GetAsync() == Task.FromResult(value.AsImmutable()));
            var controller = new CacheController(factory);

            var result = await controller.GetAsync(key).ConfigureAwait(false);

            Mock.Get(factory).VerifyAll();
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var content = Assert.IsType<byte[]>(ok.Value);
            Assert.Same(value, content);
        }

        [Fact]
        public async Task SetsKeyContent()
        {
            var key = Guid.NewGuid().ToString();
            var value = Guid.NewGuid().ToByteArray();
            var absoluteExpiration = DateTimeOffset.UtcNow.AddHours(1);
            var slidingExpiration = TimeSpan.FromMinutes(1);
            var factory = Mock.Of<IGrainFactory>(x => x.GetGrain<ICacheGrain>(key, null).SetAsync(It.Is<Immutable<byte[]>>(v => v.Value.SequenceEqual(value)), absoluteExpiration, slidingExpiration) == Task.CompletedTask);
            var controller = new CacheController(factory);

            var file = Mock.Of<IFormFile>(x => x.OpenReadStream() == new MemoryStream(value) && x.Length == value.Length);
            var result = await controller.SetAsync(key, absoluteExpiration, slidingExpiration, file).ConfigureAwait(false);

            Mock.Get(factory).VerifyAll();
            Assert.IsType<OkResult>(result);
        }
    }
}