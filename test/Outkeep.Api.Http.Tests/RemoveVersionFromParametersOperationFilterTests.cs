using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
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
            var filter = new RemoveVersionFromParametersOperationFilter();
            var operation = new OpenApiOperation
            {
                Parameters = new List<OpenApiParameter>
                {
                    new OpenApiParameter { Name = "first" },
                    new OpenApiParameter { Name = "version" },
                    new OpenApiParameter { Name = "last"}
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