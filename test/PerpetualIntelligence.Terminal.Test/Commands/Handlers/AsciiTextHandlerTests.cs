/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using PerpetualIntelligence.Terminal.Mocks;
using System;
using System.Text;
using Xunit;

namespace PerpetualIntelligence.Terminal.Commands.Handlers
{
    public class AsciiTextHandlerTests
    {
        private readonly AsciiTextHandler _handler;

        public AsciiTextHandlerTests()
        {
            _handler = new AsciiTextHandler();
        }

        [Fact]
        public void Comparison_Should_Return_InvariantCultureIgnoreCase()
        {
            //Arrange

            //Act
            var result = _handler.Comparison;

            //Assert
            result.Should().Be(StringComparison.InvariantCultureIgnoreCase);
        }

        [Fact]
        public void Encoding_Should_Return_ASCII()
        {
            //Arrange

            //Act
            var result = _handler.Encoding;

            //Assert
            result.Should().Be(Encoding.ASCII);
        }

        [Fact]
        public void EqualityComparer_Should_Return_OrdinalIgnoreCase()
        {
            //Arrange

            //Act
            var result = _handler.EqualityComparer();

            //Assert
            result.Should().BeEquivalentTo(StringComparer.OrdinalIgnoreCase);
        }

        [Fact]
        public void ExtractionRegex_Should_Return_Default_Extraction_Regex()
        {
            var result = _handler.ExtractionRegex(MockTerminalOptions.NewAliasOptions());
            result.Should().Be("(?:[^\"\\s]+)|\"[^\"]*\"|--?\\w+");
        }

        [Theory]
        [InlineData("", "", true)]
        [InlineData("", null, false)]
        [InlineData(null, "", false)]
        [InlineData("Hello", "hello", true)]
        [InlineData("Hi", "Hello", false)]
        public void TextEquals_Should_Return_Correct_Boolean(string? s1, string? s2, bool expectedResult)
        {
            //Arrange

            //Act
            var result = _handler.TextEquals(s1, s2);

            //Assert
            result.Should().Be(expectedResult);
        }

        [Theory]
        [InlineData("", 0)]
        [InlineData("test-length", 11)]
        [InlineData(null, 0)]
        public void TextLength_Should_Return_Correct_Length(string? s1, int expectedResult)
        {
            //Arrange

            //Act
            var result = _handler.TextLength(s1);

            //Assert
            result.Should().Be(expectedResult);
        }
    }
}