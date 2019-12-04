using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Options;
using Moq;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using Xunit;

namespace Outkeep.Api.Http.Tests
{
    public class SwaggerGenOptionsConfiguratorTests
    {
        [Fact]
        public void ConfiguresOptions()
        {
            // arrange
            var groupName = "V1";
            var version = new ApiVersion(1, 2);
            var title = "Test";

            var descriptions = new List<ApiVersionDescription>
            {
                new ApiVersionDescription(version, groupName, false)
            };

            var provider = Mock.Of<IApiVersionDescriptionProvider>(x =>
                x.ApiVersionDescriptions == descriptions);

            var options = new OutkeepHttpApiServerOptions
            {
                Title = title
            };

            var configurator = new SwaggerGenOptionsConfigurator(provider, Options.Create(options));

            // act
            var target = new SwaggerGenOptions();
            configurator.Configure(target);

            // assert parameters are camel case
            Assert.True(target.SwaggerGeneratorOptions.DescribeAllParametersInCamelCase);

            // assert swagger page is added
            Assert.Contains(target.SwaggerGeneratorOptions.SwaggerDocs, x => x.Key == groupName && x.Value.Version == version.ToString() && x.Value.Title == title);

            // assert version is removed from parameters
            Assert.Contains(target.OperationFilterDescriptors, x => x.Type == typeof(RemoveVersionFromParametersOperationFilter));

            // assert version is replaced in path
            Assert.Contains(target.DocumentFilterDescriptors, x => x.Type == typeof(ReplaceVersionParameterInPathDocumentFilter));

            // assert comments are enabled
            Assert.Contains(target.SchemaFilterDescriptors, x => x.Type == typeof(XmlCommentsSchemaFilter));

            // assert annotations are enabled
            Assert.Contains(target.DocumentFilterDescriptors, x => x.Type == typeof(AnnotationsDocumentFilter));
            Assert.Contains(target.OperationFilterDescriptors, x => x.Type == typeof(AnnotationsOperationFilter));
            Assert.Contains(target.ParameterFilterDescriptors, x => x.Type == typeof(AnnotationsParameterFilter));
            Assert.Contains(target.SchemaFilterDescriptors, x => x.Type == typeof(AnnotationsSchemaFilter));
        }
    }
}