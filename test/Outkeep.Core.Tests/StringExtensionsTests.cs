using System;
using System.Globalization;
using Xunit;

namespace Outkeep.Core.Tests
{
    public class StringExtensionsTests
    {
        [Fact]
        public void FormatsOneArgument()
        {
            // arrange
            var arg0 = "Zero";
            var format = "Args {0}";
            var expected = string.Format(CultureInfo.InvariantCulture, format, arg0);

            // act
            var result = format.Format(arg0);

            // assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void FormatsTwoArguments()
        {
            // arrange
            var arg0 = "Zero";
            var arg1 = "One";
            var format = "Args {0} {1}";
            var expected = string.Format(CultureInfo.InvariantCulture, format, arg0, arg1);

            // act
            var result = format.Format(arg0, arg1);

            // assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void FormatsThreeArguments()
        {
            // arrange
            var arg0 = "Zero";
            var arg1 = "One";
            var arg2 = "Two";
            var format = "Args {0} {1} {2}";
            var expected = string.Format(CultureInfo.InvariantCulture, format, arg0, arg1, arg2);

            // act
            var result = format.Format(arg0, arg1, arg2);

            // assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void FormatsParamsArguments()
        {
            // arrange
            var arg0 = "Zero";
            var arg1 = "One";
            var arg2 = "Two";
            var arg3 = "Three";
            var format = "Args {0} {1} {2} {3}";
            var expected = string.Format(CultureInfo.InvariantCulture, format, new object[] { arg0, arg1, arg2, arg3 });

            // act
            var result = format.Format(new object[] { arg0, arg1, arg2, arg3 });

            // assert
            Assert.Equal(expected, result);
        }
    }
}