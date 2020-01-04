using Outkeep.Core.Storage;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
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

        [Fact]
        private void ConstructsWithSerializationInfo()
        {
            // arrange
            var message = "Some Message";
            var inner = new InvalidOperationException();
            var exception = new StorageException(message, inner);
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
            var result = Assert.IsType<StorageException>(obj);
            Assert.Equal(message, result.Message);
            Assert.IsType<InvalidOperationException>(result.InnerException);
        }
    }
}