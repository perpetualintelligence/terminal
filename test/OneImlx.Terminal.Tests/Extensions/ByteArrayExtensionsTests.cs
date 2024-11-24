using System;
using System.Collections.Generic;
using Xunit;
using OneImlx.Terminal.Extensions;

namespace OneImlx.Terminal.Tests.Extensions
{
    public class ByteArrayExtensionsTests
    {
        [Fact]
        public void Split_ShouldThrowArgumentException_WhenDelimiterIsNull()
        {
            byte[] source = new byte[] { 1, 2, 3, 4, 5 };
            byte[] delimiter = null;

            Assert.Throws<ArgumentException>(() => source.Split(delimiter));
        }

        [Fact]
        public void Split_ShouldThrowArgumentException_WhenDelimiterIsEmpty()
        {
            byte[] source = new byte[] { 1, 2, 3, 4, 5 };
            byte[] delimiter = new byte[] { };

            Assert.Throws<ArgumentException>(() => source.Split(delimiter));
        }

        [Fact]
        public void Split_ShouldReturnSingleSegment_WhenDelimiterNotFound()
        {
            byte[] source = new byte[] { 1, 2, 3, 4, 5 };
            byte[] delimiter = new byte[] { 6 };

            var result = source.Split(delimiter);

            Assert.Single(result);
            Assert.Equal(source, result[0]);
        }

        [Fact]
        public void Split_ShouldReturnMultipleSegments_WhenDelimiterFound()
        {
            byte[] source = new byte[] { 1, 2, 3, 4, 5, 2, 3, 6 };
            byte[] delimiter = new byte[] { 2, 3 };

            var result = source.Split(delimiter);

            Assert.Equal(3, result.Count);
            Assert.Equal(new byte[] { 1 }, result[0]);
            Assert.Equal(new byte[] { 4, 5 }, result[1]);
            Assert.Equal(new byte[] { 6 }, result[2]);
        }

        [Fact]
        public void Split_ShouldHandleDelimiterAtStart()
        {
            byte[] source = new byte[] { 2, 3, 1, 2, 3, 4, 5 };
            byte[] delimiter = new byte[] { 2, 3 };

            var result = source.Split(delimiter);

            Assert.Equal(3, result.Count);
            Assert.Empty(result[0]);
            Assert.Equal(new byte[] { 1 }, result[1]);
            Assert.Equal(new byte[] { 4, 5 }, result[2]);
        }

        [Fact]
        public void Split_ShouldHandleDelimiterAtEnd()
        {
            byte[] source = new byte[] { 1, 2, 3, 4, 5, 2, 3 };
            byte[] delimiter = new byte[] { 2, 3 };

            var result = source.Split(delimiter);

            Assert.Equal(2, result.Count);
            Assert.Equal(new byte[] { 1 }, result[0]);
            Assert.Equal(new byte[] { 4, 5 }, result[1]);
        }

        [Fact]
        public void Split_ShouldHandleConsecutiveDelimiters()
        {
            byte[] source = new byte[] { 1, 2, 3, 2, 3, 4, 5 };
            byte[] delimiter = new byte[] { 2, 3 };

            var result = source.Split(delimiter);

            Assert.Equal(3, result.Count);
            Assert.Equal(new byte[] { 1 }, result[0]);
            Assert.Empty(result[1]);
            Assert.Equal(new byte[] { 4, 5 }, result[2]);
        }
    }
}
