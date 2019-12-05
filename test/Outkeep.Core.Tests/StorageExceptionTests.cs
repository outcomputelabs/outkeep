using Xunit;

namespace Outkeep.Core.Tests
{
    public class StorageExceptionTests
    {
        [Fact]
        public void Constructs()
        {
            // act
            var exception = new StorageException();

            // assert
            Assert.NotNull(exception.Message);
            Assert.Null(exception.InnerException);
        }
    }
}