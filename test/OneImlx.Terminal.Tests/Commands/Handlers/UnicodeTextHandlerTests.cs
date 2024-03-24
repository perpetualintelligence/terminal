/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using OneImlx.Terminal.Mocks;
using OneImlx.Terminal.Runtime;
using System;
using System.Text;
using Xunit;

namespace OneImlx.Terminal.Commands.Handlers
{
    public class UnicodeTextHandlerTests
    {
        private readonly TerminalUnicodeTextHandler _textHandler = new();

        [Theory]
        [InlineData("नमस्ते", "नमस्ते", true)] // Equal
        [InlineData("नमस्ते", "नमस्कार", false)] // Not equal
        public void TextEquals_ShouldCompareUnicodeTexts(string text1, string text2, bool expected)
        {
            bool result = _textHandler.TextEquals(text1, text2);
            result.Should().Be(expected);
        }

        [Theory]
        [InlineData("नमस्ते", 4)] // Length = 4 characters
        [InlineData("वनकमन", 5)] // Length = 5 characters
        public void TextLength_ShouldCalculateUnicodeTextLength(string text, int expectedLength)
        {
            int result = _textHandler.TextLength(text);
            result.Should().Be(expectedLength);
        }

        [Fact]
        public void Comparison_ShouldBeOrdinalIgnoreCase()
        {
            StringComparison comparison = _textHandler.Comparison;
            comparison.Should().Be(StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Encoding_ShouldBeUnicode()
        {
            Encoding encoding = _textHandler.Encoding;
            encoding.Should().Be(Encoding.Unicode);
        }

        [Fact]
        public void ExtractionRegex_ShouldReturnValidPattern()
        {
            string pattern = _textHandler.ExtractionRegex(MockTerminalOptions.NewAliasOptions());
            pattern.Should().Be("(\\w+)|((?:--|-)\\w+(?:\\s+\"[^\"]*\")*)");
        }

        [Fact]
        public void EqualityComparer_ShouldBeOrdinalIgnoreCase()
        {
            var comparer = _textHandler.EqualityComparer();
            comparer.Should().BeEquivalentTo(StringComparer.OrdinalIgnoreCase);
        }
    }
}