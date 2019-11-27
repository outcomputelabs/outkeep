using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Moq;
using System;
using Xunit;

namespace Outkeep.Api.Http.Tests
{
    public class OrleansActivityMiddlewareExtensionsTests
    {
        [Fact]
        public void UseActivityMiddleware()
        {
            var builder = Mock.Of<IApplicationBuilder>();
            Mock.Get(builder).Setup(x => x.Use(It.IsAny<Func<RequestDelegate, RequestDelegate>>())).Returns(builder);

            var result = builder.UseActivityMiddleware();

            Mock.Get(builder).VerifyAll();
            Assert.Same(builder, result);
        }
    }
}