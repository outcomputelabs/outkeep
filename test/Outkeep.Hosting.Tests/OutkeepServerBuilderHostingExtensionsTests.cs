using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Moq;
using System;
using Xunit;

namespace Outkeep.Hosting.Tests
{
    public class OutkeepServerBuilderHostingExtensionsTests
    {
        private class MyOptions
        {
            public int SomeProperty { get; set; }
        }

        [Fact]
        public void ConfiguresOptions()
        {
            var services = new ServiceCollection();

            var builder = Mock.Of<IOutkeepServerBuilder>();
            Mock.Get(builder)
                .Setup(x => x.ConfigureServices(It.IsAny<Action<HostBuilderContext, IServiceCollection>>()))
                .Callback((Action<HostBuilderContext, IServiceCollection> action) => action(null, services))
                .Returns(builder);

            builder.Configure<MyOptions>(options =>
            {
                options.SomeProperty = 123;
            });

            var options = services.BuildServiceProvider().GetService<IOptions<MyOptions>>();
            Assert.NotNull(options);
            Assert.Equal(123, options.Value.SomeProperty);

            Mock.Get(builder).VerifyNoOtherCalls();
        }
    }
}