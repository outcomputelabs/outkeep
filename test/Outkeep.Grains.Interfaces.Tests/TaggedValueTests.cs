using System;
using Xunit;

namespace Outkeep.Grains.Interfaces.Tests
{
    public class TaggedValueTests
    {
        [Fact]
        public void PropertiesShowCorrectValues()
        {
            // arrange
            Guid tag = Guid.NewGuid(), value = Guid.NewGuid();

            // act
            var item = new TaggedValue<Guid, Guid>(tag, value);

            // assert
            Assert.Equal(tag, item.Tag);
            Assert.Equal(value, item.Value);
        }

        [Fact]
        public void MethodEquals()
        {
            // arrange
            Guid tag = Guid.NewGuid(), value = Guid.NewGuid();

            // act
            var item1 = new TaggedValue<Guid, Guid>(tag, value);
            var item2 = new TaggedValue<Guid, Guid>(tag, value);
            var item3 = new TaggedValue<Guid, Guid>(tag, Guid.NewGuid());
            var item4 = new TaggedValue<Guid, Guid>(Guid.NewGuid(), value);

            // assert
            Assert.True(item1.Equals(item2));
            Assert.True(item2.Equals(item1));
            Assert.False(item1.Equals(item3));
            Assert.False(item1.Equals(item4));
        }

        [Fact]
        public void OperatorEquals()
        {
            // arrange
            Guid tag = Guid.NewGuid(), value = Guid.NewGuid();

            // act
            var item1 = new TaggedValue<Guid, Guid>(tag, value);
            var item2 = new TaggedValue<Guid, Guid>(tag, value);
            var item3 = new TaggedValue<Guid, Guid>(tag, Guid.NewGuid());
            var item4 = new TaggedValue<Guid, Guid>(Guid.NewGuid(), value);

            // assert
            Assert.True(item1 == item2);
            Assert.True(item2 == item1);
            Assert.False(item1 == item3);
            Assert.False(item1 == item4);
        }

        [Fact]
        public void OperatorNotEquals()
        {
            // arrange
            Guid tag = Guid.NewGuid(), value = Guid.NewGuid();

            // act
            var item1 = new TaggedValue<Guid, Guid>(tag, value);
            var item2 = new TaggedValue<Guid, Guid>(tag, value);
            var item3 = new TaggedValue<Guid, Guid>(tag, Guid.NewGuid());
            var item4 = new TaggedValue<Guid, Guid>(Guid.NewGuid(), value);

            // assert
            Assert.False(item1 != item2);
            Assert.False(item2 != item1);
            Assert.True(item1 != item3);
            Assert.True(item1 != item4);
        }

        [Fact]
        public void MethodGetHashCode()
        {
            // arrange
            var tag = Guid.NewGuid();
            var value = Guid.NewGuid();
            var item = new TaggedValue<Guid, Guid>(tag, value);

            // act
            var hash = item.GetHashCode();

            // assert
            Assert.Equal(HashCode.Combine(tag, value), hash);
        }
    }
}