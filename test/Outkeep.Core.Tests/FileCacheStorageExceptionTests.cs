using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Xunit;

namespace Outkeep.Core.Tests
{
    public class FileCacheStorageExceptionTests
    {
        [Fact]
        public void Constructs()
        {
            // arrange
            var exception = new FileCacheStorageException();

            // assert
            Assert.Null(exception.Path);
            Assert.Null(exception.Key);
            Assert.Null(exception.OtherKey);
            Assert.NotNull(exception.Message);
        }

        [Fact]
        public void ConstructsWithMessage()
        {
            // arrange
            var message = "SomeMessage";
            var exception = new FileCacheStorageException(message);

            // assert
            Assert.Null(exception.Path);
            Assert.Null(exception.Key);
            Assert.Null(exception.OtherKey);
            Assert.Equal(message, exception.Message);
        }

        [Fact]
        public void ConstructsWithBaseParameters()
        {
            // arrange
            var message = "SomeMessage";
            var path = "SomePath";
            var key = "SomeKey";
            var exception = new FileCacheStorageException(message, path, key);

            // assert
            Assert.Equal(path, exception.Path);
            Assert.Equal(key, exception.Key);
            Assert.Null(exception.OtherKey);
            Assert.Equal(message, exception.Message);
        }

        [Fact]
        public void ConstructsWithExtraParameters()
        {
            // arrange
            var message = "SomeMessage";
            var path = "SomePath";
            var key = "SomeKey";
            var otherKey = "SomeOtherKey";
            var exception = new FileCacheStorageException(message, path, key, otherKey);

            // assert
            Assert.Equal(path, exception.Path);
            Assert.Equal(key, exception.Key);
            Assert.Equal(otherKey, exception.OtherKey);
            Assert.Equal(message, exception.Message);
        }

        [Fact]
        public void ConstructsWithMessageAndInnerException()
        {
            // arrange
            var message = "SomeMessage";
            var inner = new Exception();
            var exception = new FileCacheStorageException(message, inner);

            // assert
            Assert.Null(exception.Path);
            Assert.Null(exception.Key);
            Assert.Null(exception.OtherKey);
            Assert.Same(inner, exception.InnerException);
            Assert.Equal(message, exception.Message);
        }

        [Fact]
        public void ConstructsWithBaseParametersAndInnerException()
        {
            // arrange
            var message = "SomeMessage";
            var path = "SomePath";
            var key = "SomeKey";
            var inner = new Exception();
            var exception = new FileCacheStorageException(message, path, key, inner);

            // assert
            Assert.Equal(path, exception.Path);
            Assert.Equal(key, exception.Key);
            Assert.Null(exception.OtherKey);
            Assert.Same(inner, exception.InnerException);
            Assert.Equal(message, exception.Message);
        }

        [Fact]
        public void ConstructsWithExtraParametersAndInnerException()
        {
            // arrange
            var message = "SomeMessage";
            var path = "SomePath";
            var key = "SomeKey";
            var otherKey = "SomeOtherKey";
            var inner = new Exception();
            var exception = new FileCacheStorageException(message, path, key, otherKey, inner);

            // assert
            Assert.Equal(path, exception.Path);
            Assert.Equal(key, exception.Key);
            Assert.Equal(otherKey, exception.OtherKey);
            Assert.Same(inner, exception.InnerException);
            Assert.Equal(message, exception.Message);
        }

        [Fact]
        public void CanSerialize()
        {
            // act
            var message = "SomeMessage";
            var path = "SomePath";
            var key = "SomeKey";
            var otherKey = "SomeOtherKey";
            var inner = new Exception("Some Inner Exception");
            var exception = new FileCacheStorageException(message, path, key, otherKey, inner);
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
            var result = Assert.IsType<FileCacheStorageException>(obj);
            Assert.Equal(path, result.Path);
            Assert.Equal(key, result.Key);
            Assert.Equal(otherKey, result.OtherKey);
            Assert.Equal(inner.Message, result.InnerException?.Message);
            Assert.Equal(message, result.Message);
        }

        [Fact]
        public void ToStringOutputsString()
        {
            // arrange
            var key = "SomeKey";
            var otherKey = "SomeOtherKey";
            var path = "SomePath";
            var message = "SomeMessage";
            var innerException = new Exception("Some Inner Exception Message");
            var expected = $"{nameof(FileCacheStorageException)}: Key='{key}', OtherKey='{otherKey}', Path='{path}', Message='{message}', InnerException='{innerException}'";
            var exception = new FileCacheStorageException(message, path, key, otherKey, innerException);

            // act
            var result = exception.ToString();

            // assert
            Assert.Equal(expected, result);
        }
    }
}