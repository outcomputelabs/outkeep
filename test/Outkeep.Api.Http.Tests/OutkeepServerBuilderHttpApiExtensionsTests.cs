using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Orleans;
using Outkeep.Core;
using System;
using Xunit;

namespace Outkeep.Api.Http.Tests
{
    public class OutkeepServerBuilderHttpApiExtensionsTests
    {
        [Fact]
        public void UseHttpApi()
        {
            var services = new ServiceCollection();
            services.AddSingleton(Mock.Of<ILogger<OutkeepHttpApiHostedService>>());
            services.AddSingleton(Mock.Of<IGrainFactory>());

            var builder = Mock.Of<IOutkeepServerBuilder>();
            Mock.Get(builder)
                .Setup(x => x.ConfigureServices(It.IsAny<Action<HostBuilderContext, IServiceCollection>>()))
                .Callback((Action<HostBuilderContext, IServiceCollection> action) => action(null!, services))
                .Returns(builder);

            builder.UseHttpApi(options =>
            {
                options.ApiUri = new Uri("http://localhost:12345");
                options.Title = "Test";
            });

            var provider = services.BuildServiceProvider();

            var options = provider.GetService<IOptions<OutkeepHttpApiServerOptions>>().Value;
            Assert.NotNull(options);
            Assert.Equal(new Uri("http://localhost:12345"), options.ApiUri);
            Assert.Equal("Test", options.Title);

            var validator = provider.GetService<IValidateOptions<OutkeepHttpApiServerOptions>>();
            Assert.NotNull(validator);

            var hosted = provider.GetService<IHostedService>();
            Assert.NotNull(hosted);
            Assert.IsType<OutkeepHttpApiHostedService>(hosted);
        }
    }
}