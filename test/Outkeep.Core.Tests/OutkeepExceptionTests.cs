using Xunit;

namespace Outkeep.Core.Tests
{
    public class OutkeepExceptionTests
    {
        [Fact]
        public void Constructs()
        {
            // arrange
            var exception = new OutkeepException();

            // assert
            Assert.NotNull(exception.Message);
            Assert.Null(exception.InnerException);
        }
    }
}