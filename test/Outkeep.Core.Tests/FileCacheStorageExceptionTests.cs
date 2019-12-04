using System;
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

        [Fact]
        public void ConstructsWithMessage()
        {
            // act
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
            // act
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
            // act
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
            // act
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
            // act
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
            // act
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