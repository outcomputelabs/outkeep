using System;
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

        [Fact]
        public void ConstructsWithMessage()
        {
            // arrange
            var message = "Some Message";

            // act
            var exception = new StorageException(message);

            // assert
            Assert.Equal(message, exception.Message);
            Assert.Null(exception.InnerException);
        }

        [Fact]
        public void ConstructsWithMessageAndInnerException()
        {
            // arrange
            var message = "Some Message";
            var inner = new InvalidOperationException();

            // act
            var exception = new StorageException(message, inner);

            // assert
            Assert.Equal(message, exception.Message);
            Assert.Same(inner, exception.InnerException);
        }
    }
}