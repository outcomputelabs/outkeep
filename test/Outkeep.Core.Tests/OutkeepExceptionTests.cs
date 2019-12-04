using System;
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

        [Fact]
        public void ConstructsWithMessage()
        {
            // arrange
            var message = "Some Message";
            var exception = new OutkeepException(message);

            // assert
            Assert.Equal(message, exception.Message);
            Assert.Null(exception.InnerException);
        }

        [Fact]
        public void ConstructsWithMessageAndInnerException()
        {
            // arrange
            var message = "Some Message";
            var inner = new Exception("Some Inner Exception");
            var exception = new OutkeepException(message, inner);

            // assert
            Assert.Equal(message, exception.Message);
            Assert.Same(inner, exception.InnerException);
        }
    }
}