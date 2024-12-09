/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Text;
using FluentAssertions;
using OneImlx.Terminal.Mocks;
using OneImlx.Terminal.Runtime;
using Xunit;

namespace OneImlx.Terminal.Commands.Handlers
{
    public class UnicodeTextHandlerTests
    {
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
        public void EqualityComparer_ShouldBeOrdinalIgnoreCase()
        {
            var comparer = _textHandler.EqualityComparer();
            comparer.Should().BeEquivalentTo(StringComparer.OrdinalIgnoreCase);
        }

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

        private readonly TerminalUnicodeTextHandler _textHandler = new();
    }
}
