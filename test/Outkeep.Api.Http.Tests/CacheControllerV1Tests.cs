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
        private static readonly byte[] empty = null;

        [Fact]
        public async Task GetNonExistingKeyReturnsNoContent()
        {
            var key = Guid.NewGuid().ToString();
            var factory = Mock.Of<IGrainFactory>(x => x.GetGrain<ICacheGrain>(key, null).GetAsync() == Task.FromResult(empty.AsImmutable()));
            var controller = new CacheController(factory);

            var result = await controller.GetAsync(key).ConfigureAwait(false);

            Assert.IsType<NoContentResult>(result.Result);
        }
    }
}