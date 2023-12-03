/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Xunit;

namespace OneImlx.Terminal.Commands.Parsers
{
    public class ParsedSplitTests
    {
        [Fact]
        public void ParsedSplit_Sets_Properties_As_Expected()
        {
            // Arrange
            var split = "test";
            var token = "token";
            var parsedSplit = new ParsedSplit(split, token);
            parsedSplit.Split.Should().Be(split);
            parsedSplit.Token.Should().Be(token);
        }

        [Fact]
        public void ParsedSplit_SplitAndToken_ReturnsExpectedString()
        {
            // Arrange
            var split = "test";
            var token = "token";
            var parsedSplit = new ParsedSplit(split, token);

            // Act
            var result = parsedSplit.ToString();

            // Assert
            result.Should().Be($"{split} - {token}");
        }

        [Fact]
        public void ParsedSplit_SplitOnly_ReturnsExpectedString()
        {
            // Arrange
            var split = "test";
            var parsedSplit = new ParsedSplit(split, null);

            // Act
            var result = parsedSplit.ToString();

            // Assert
            result.Should().Be(split);
        }
    }
}