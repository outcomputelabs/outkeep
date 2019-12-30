using Outkeep.Core.Properties;
using Xunit;

namespace Outkeep.Core.Tests
{
    public class FileCacheStorageOptionsValidatorTests
    {
        [Fact]
        public void RequiresStorageDirectory()
        {
            // arrange
            var options = new FileCacheStorageOptions();
            var validator = new FileCacheStorageOptionsValidator();

            // act
            var result = validator.Validate(null!, options);

            // assert
            Assert.True(result.Failed);
            Assert.Equal(Resources.Exception_CacheStorageOptions_StorageDirectory_MustBeConfigured, result.FailureMessage);
        }
    }
}