using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
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

        [Fact]
        public void CanSerialize()
        {
            // act
            var message = "Some Message";
            var inner = new Exception("Some Inner Exception");
            var exception = new OutkeepException(message, inner);
            var formatter = new BinaryFormatter();

            // act
            object obj;
            using (var stream = new MemoryStream())
            {
                formatter.Serialize(stream, exception);
                stream.Position = 0;
                obj = formatter.Deserialize(stream);
            }

            // assert
            Assert.NotNull(obj);
            var result = Assert.IsType<OutkeepException>(obj);
            Assert.Equal(inner.Message, result.InnerException.Message);
            Assert.Equal(message, result.Message);
        }
    }
}