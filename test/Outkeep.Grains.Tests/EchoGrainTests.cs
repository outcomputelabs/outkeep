using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Outkeep.Grains.Tests
{
    public class EchoGrainTests
    {
        [Fact]
        public async Task Echoes()
        {
            var grain = new EchoGrain(NullLogger<EchoGrain>.Instance);
            var message = Guid.NewGuid().ToString();

            await grain.OnActivateAsync().ConfigureAwait(false);
            var result = await grain.EchoAsync(message).ConfigureAwait(false);

            Assert.Equal(message, result);
        }
    }
}