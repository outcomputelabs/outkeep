using Microsoft.Extensions.DependencyInjection;
using Outkeep.Core.Tests.Fakes;
using System;
using Xunit;

namespace Outkeep.Core.Tests
{
    public class OutkeepServiceCollectionExtensionsTests
    {
        [Fact]
        public void TryAddHostedServiceThrowsOnNullServices()
        {
            Assert.Throws<ArgumentNullException>("services", () => OutkeepServiceCollectionExtensions.TryAddHostedService<FakeHostedService>(null!));
        }
    }
}