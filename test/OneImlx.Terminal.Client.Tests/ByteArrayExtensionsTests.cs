/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using FluentAssertions;
using Xunit;

namespace OneImlx.Terminal.Extensions.Tests
{
    public class ByteArrayExtensionsTests
    {
        [Fact]
        public void Split_HandlesConsecutiveDelimiters()
        {
            // Arrange
            byte[] source = [1, 0x1F, 0x1F, 2, 3];
            byte delimiter = 0x1F;

            // Act
            var result = source.Split(delimiter, false, out var endsWithDelimiter);

            // Assert
            result.Should().HaveCount(3);
            result[0].Should().BeEquivalentTo(new byte[] { 1 });
            result[1].Should().BeEmpty();
            result[2].Should().BeEquivalentTo(new byte[] { 2, 3 });
            endsWithDelimiter.Should().BeFalse();
        }

        [Fact]
        public void Split_HandlesConsecutiveDelimiters_IgnoreEmpty()
        {
            // Arrange
            byte[] source = [1, 0x1F, 0x1F, 2, 3];
            byte delimiter = 0x1F;

            // Act
            var result = source.Split(delimiter, true, out var endsWithDelimiter);

            // Assert
            result.Should().HaveCount(2);
            result[0].Should().BeEquivalentTo(new byte[] { 1 });
            result[1].Should().BeEquivalentTo(new byte[] { 2, 3 });
            endsWithDelimiter.Should().BeFalse();
        }

        [Fact]
        public void Split_HandlesDelimiterAtStartAndEnd()
        {
            // Arrange
            byte[] source = [0x1F, 1, 2, 3, 0x1F];
            byte delimiter = 0x1F;

            // Act
            var result = source.Split(delimiter, false, out var endsWithDelimiter);

            // Assert
            result.Should().HaveCount(3);
            result[0].Should().BeEmpty();
            result[1].Should().BeEquivalentTo(new byte[] { 1, 2, 3 });
            result[2].Should().BeEmpty();
            endsWithDelimiter.Should().BeTrue();
        }

        [Fact]
        public void Split_HandlesDelimiterAtStartAndEnd_IgnoreEmpty()
        {
            // Arrange
            byte[] source = [0x1F, 1, 2, 3, 0x1F];
            byte delimiter = 0x1F;

            // Act
            var result = source.Split(delimiter, true, out var endsWithDelimiter);

            // Assert
            result.Should().HaveCount(1);
            result[0].Should().BeEquivalentTo(new byte[] { 1, 2, 3 });
            endsWithDelimiter.Should().BeTrue();
        }

        [Fact]
        public void Split_ReturnsEmptyLastSegment_WhenSourceEndsWithDelimiter()
        {
            // Arrange
            byte[] source = [1, 2, 3, 4, 5, 0x1F];
            byte delimiter = 0x1F;

            // Act
            var result = source.Split(delimiter, false, out var endsWithDelimiter);

            // Assert
            result.Should().HaveCount(2);
            result[0].Should().BeEquivalentTo(new byte[] { 1, 2, 3, 4, 5 });
            result[1].Should().BeEmpty();
            endsWithDelimiter.Should().BeTrue();
        }

        [Fact]
        public void Split_ReturnsEmptyLastSegment_WhenSourceEndsWithDelimiter_IgnoreEmpty()
        {
            // Arrange
            byte[] source = [1, 2, 3, 4, 5, 0x1F];
            byte delimiter = 0x1F;

            // Act
            var result = source.Split(delimiter, true, out var endsWithDelimiter);

            // Assert
            result.Should().HaveCount(1);
            result[0].Should().BeEquivalentTo(new byte[] { 1, 2, 3, 4, 5 });
            endsWithDelimiter.Should().BeTrue();
        }

        [Fact]
        public void Split_ReturnsNoSegments_WhenOnlyDelimiterExists_IgnoreEmpty()
        {
            // Arrange
            byte[] source = [0x1F];
            byte delimiter = 0x1F;

            // Act
            var result = source.Split(delimiter, true, out var endsWithDelimiter);

            // Assert
            result.Should().BeEmpty();
            endsWithDelimiter.Should().BeTrue();
        }

        [Fact]
        public void Split_ReturnsSingleEmptySegment_WhenOnlyDelimiterExists()
        {
            // Arrange
            byte[] source = [0x1F];
            byte delimiter = 0x1F;

            // Act
            var result = source.Split(delimiter, false, out var endsWithDelimiter);

            // Assert
            result.Should().HaveCount(1);
            result[0].Should().BeEmpty();
            endsWithDelimiter.Should().BeTrue();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Split_ReturnsSingleSegment_WhenDelimiterIsAbsent(bool ignoreEmpty)
        {
            // Arrange
            byte[] source = [1, 2, 3, 4, 5];
            byte delimiter = 0x1F;

            // Act
            var result = source.Split(delimiter, ignoreEmpty, out var endsWithDelimiter);

            // Assert
            result.Should().HaveCount(1);
            result[0].Should().BeEquivalentTo(source);
            endsWithDelimiter.Should().BeFalse();
        }

        [Fact]
        public void Split_ThrowsArgumentException_WhenSourceIsEmpty()
        {
            // Arrange
            byte[] source = [];
            byte delimiter = 0x1F;

            // Act
            Action act = () => source.Split(delimiter, false, out _);

            // Assert
            act.Should().Throw<ArgumentException>()
               .WithMessage("Source array cannot be null or empty.*");
        }

        [Fact]
        public void Split_ThrowsArgumentException_WhenSourceIsNull()
        {
            // Arrange
            byte[] source = null!;
            byte delimiter = 0x1F;

            // Act
            Action act = () => source.Split(delimiter, false, out _);

            // Assert
            act.Should().Throw<ArgumentException>()
               .WithMessage("Source array cannot be null or empty.*");
        }
    }
}
