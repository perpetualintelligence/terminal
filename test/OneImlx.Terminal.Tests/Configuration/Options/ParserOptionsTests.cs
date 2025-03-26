/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using OneImlx.Terminal.Shared;
using Xunit;

namespace OneImlx.Terminal.Configuration.Options
{
    public class ParserOptionsTests
    {
        [Fact]
        public void ExtractorOptionsShouldHaveCorrectDefaultValues()
        {
            ParserOptions options = new();

            options.OptionPrefix.Should().Be('-');
            options.OptionValueSeparator.Should().Be(TerminalIdentifiers.SpaceSeparator);
            options.ValueDelimiter.Should().Be('"');
            options.Separator.Should().Be(TerminalIdentifiers.SpaceSeparator);
            options.RuntimeSeparator.Should().Be('\u001F');
        }
    }
}
