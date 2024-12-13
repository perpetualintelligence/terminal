/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Text;
using FluentAssertions;
using Xunit;

namespace OneImlx.Terminal.Runtime
{
    public class TerminalTextHandlerTests
    {
        public TerminalTextHandlerTests()
        {
            _textHandler = new TerminalTextHandler(StringComparison.CurrentCultureIgnoreCase, Encoding.UTF8);
        }

        [Theory]
        [InlineData('a', 'A', false)]
        [InlineData('a', 'a', true)]
        [InlineData('a', 'b', false)]
        public void CharEquals_ShouldCompareChars(char ch1, char ch2, bool expected)
        {
            bool result = _textHandler.CharEquals(ch1, ch2);
            result.Should().Be(expected);
        }

        [Fact]
        public void Constructor_Sets_Correctly()
        {
            TerminalTextHandler textHandler = new(StringComparison.CurrentCultureIgnoreCase, Encoding.UTF8);
            textHandler.Comparison.Should().Be(StringComparison.CurrentCultureIgnoreCase);
            textHandler.Encoding.Should().Be(Encoding.UTF8);
        }

        [Theory]
        [InlineData('a', "a", true)] // Equal
        [InlineData('a', "b", false)] // Not equal
        public void SingleEquals_ShouldCompareCharAndString(char ch1, string s2, bool expected)
        {
            bool result = _textHandler.SingleEquals(ch1, s2);
            result.Should().Be(expected);
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
        [InlineData("नमस्ते", 6)] // Length = 6 characters
        [InlineData("वनकमन", 5)] // Length = 7 characters
        public void TextLength_ShouldCalculateUnicodeTextLength(string text, int expectedLength)
        {
            int result = _textHandler.TextLength(text);
            result.Should().Be(expectedLength);
        }

        private TerminalTextHandler _textHandler;
    }
}
