/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Xunit;

namespace OneImlx.Terminal.Configuration.Options
{
    public class ParserOptionsTests
    {
        [Fact]
        public void ExtractorOptionsShouldHaveCorrectDefaultValues()
        {
            ParserOptions options = new();

            options.OptionAliasPrefix.Should().Be("-");
            options.OptionPrefix.Should().Be("--");
            options.OptionValueSeparator.Should().Be(" ");
            options.ValueDelimiter.Should().Be("\"");
            options.Separator.Should().Be(" ");
        }
    }
}