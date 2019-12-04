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
    }
}