using Microsoft.Extensions.Options;
using Moq;
using Swashbuckle.AspNetCore.Swagger;
using System.Collections.Generic;
using Xunit;

namespace Outkeep.Api.Http.Tests
{
    public class RemoveVersionFromParametersOperationFilterTests
    {
        [Fact]
        public void Applies()
        {
            // arrange
            var options = new RestApiServerOptions
            {
                VersionParameterName = "version"
            };
            var filter = new RemoveVersionFromParametersOperationFilter(Options.Create(options));
            var operation = new Operation
            {
                Parameters = new List<IParameter>
                {
                    Mock.Of<IParameter>(x => x.Name == "first"),
                    Mock.Of<IParameter>(x => x.Name == options.VersionParameterName),
                    Mock.Of<IParameter>(x => x.Name == "last")
                }
            };

            // act
            filter.Apply(operation, null);

            // assert
            Assert.Collection(
                operation.Parameters,
                x => Assert.Equal("first", x.Name),
                x => Assert.Equal("last", x.Name));
        }
    }
}