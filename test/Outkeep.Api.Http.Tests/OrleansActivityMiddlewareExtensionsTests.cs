using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
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

        [Fact]
        public void AddActivityMiddleware()
        {
            var services = new ServiceCollection();

            var result = services.AddActivityMiddleware();

            Assert.Same(services, result);
            Assert.Single(services, x => x.ServiceType == typeof(OrleansActivityMiddleware) && x.ImplementationType == typeof(OrleansActivityMiddleware) && x.Lifetime == ServiceLifetime.Transient);
        }

        [Fact]
        public void TryAddActivityMiddlewareAddsMiddleware()
        {
            var services = new ServiceCollection();

            var result = services.TryAddActivityMiddleware();

            Assert.Same(services, result);
            Assert.Single(services, x => x.ServiceType == typeof(OrleansActivityMiddleware) && x.ImplementationType == typeof(OrleansActivityMiddleware) && x.Lifetime == ServiceLifetime.Transient);
        }

        [Fact]
        public void TryAddActivityMiddlewareSkipsAdding()
        {
            var services = new ServiceCollection();
            services.AddActivityMiddleware();

            var result = services.TryAddActivityMiddleware();

            Assert.Same(services, result);
            Assert.Single(services, x => x.ServiceType == typeof(OrleansActivityMiddleware) && x.ImplementationType == typeof(OrleansActivityMiddleware) && x.Lifetime == ServiceLifetime.Transient);
        }
    }
}