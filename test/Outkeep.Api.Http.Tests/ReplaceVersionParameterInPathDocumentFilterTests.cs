using Microsoft.OpenApi.Models;
using Xunit;

namespace Outkeep.Api.Http.Tests
{
    public class ReplaceVersionParameterInPathDocumentFilterTests
    {
        [Fact]
        public void Applies()
        {
            var filter = new ReplaceVersionParameterInPathDocumentFilter();
            var document = new OpenApiDocument()
            {
                Paths = new OpenApiPaths
                {
                    {"Path1", new OpenApiPathItem { Description = "First" } },
                    {"Path{version}", new OpenApiPathItem { Description = "Second" } },
                    {"Path3", new OpenApiPathItem { Description = "Third"} }
                },
                Info = new OpenApiInfo
                {
                    Version = "2"
                }
            };

            filter.Apply(document, null!);

            Assert.Collection(document.Paths,
                x =>
                {
                    Assert.Equal("Path1", x.Key);
                    Assert.Equal("First", x.Value.Description);
                },
                x =>
                {
                    Assert.Equal("Path2", x.Key);
                    Assert.Equal("Second", x.Value.Description);
                },
                x =>
                {
                    Assert.Equal("Path3", x.Key);
                    Assert.Equal("Third", x.Value.Description);
                });
        }
    }
}