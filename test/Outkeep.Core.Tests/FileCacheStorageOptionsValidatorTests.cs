using Xunit;

namespace Outkeep.Core.Tests
{
    public class FileCacheStorageOptionsValidatorTests
    {
        [Fact]
        public void ValidatesDefaultOptions()
        {
            var options = new FileCacheStorageOptions();

            var validator = new FileCacheStorageOptionsValidator();
            var result = validator.Validate(null, options);

            Assert.True(result.Succeeded);
        }
    }
}