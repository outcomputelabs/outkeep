using Xunit;

namespace Outkeep.Core.Tests
{
    public class FileCacheStorageExceptionTests
    {
        [Fact]
        public void Constructs()
        {
            // act
            var exception = new FileCacheStorageException();

            // assert
            Assert.Null(exception.Path);
            Assert.Null(exception.Key);
            Assert.Null(exception.OtherKey);
            Assert.NotNull(exception.Message);
        }
    }
}