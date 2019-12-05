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
            var format = "Arg {0}";
            var expected = string.Format(CultureInfo.InvariantCulture, format, arg0);

            // act
            var result = format.Format(arg0);

            // assert
            Assert.Equal(expected, result);
        }
    }
}