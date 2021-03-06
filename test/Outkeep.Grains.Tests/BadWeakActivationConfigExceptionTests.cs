﻿using Outkeep.Governance;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Xunit;

namespace Outkeep.Grains.Tests
{
    public class BadWeakActivationConfigExceptionTests
    {
        [Fact]
        public void DefaultConstructorWorks()
        {
            // act
            var exception = new BadWeakActivationConfigException();

            // assert
            Assert.Equal($"Exception of type '{typeof(BadWeakActivationConfigException).FullName}' was thrown.", exception.Message);
            Assert.Null(exception.InnerException);
        }

        [Fact]
        [SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters")]
        public void MessageConstructorWorks()
        {
            // act
            var message = "Some Message";
            var exception = new BadWeakActivationConfigException(message);

            // assert
            Assert.Equal(message, exception.Message);
            Assert.Null(exception.InnerException);
        }

        [Fact]
        [SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters")]
        public void FullConstructorWorks()
        {
            // act
            var message = "Some Message";
            var inner = new InvalidOperationException();
            var exception = new BadWeakActivationConfigException(message, inner);

            // assert
            Assert.Equal(message, exception.Message);
            Assert.Same(inner, exception.InnerException);
        }

        [Fact]
        [SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters")]
        public void SerializationConstructorWorks()
        {
            // arrange
            var message = "Some Message";
            var inner = new InvalidOperationException();
            var exception = new BadWeakActivationConfigException(message, inner);

            // act
            var formatter = new BinaryFormatter();
            using var stream = new MemoryStream();

            formatter.Serialize(stream, exception);

            stream.Position = 0;
            var output = formatter.Deserialize(stream);

            // assert
            var other = Assert.IsType<BadWeakActivationConfigException>(output);
            Assert.Equal(message, other.Message);
            Assert.Equal(inner.GetType(), other.InnerException?.GetType());
        }
    }
}