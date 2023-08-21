/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Xunit;

namespace PerpetualIntelligence.Terminal.Configuration.Options
{
    public class ExtractorOptionsTests
    {
        [Fact]
        public void ExtractorOptionsShouldHaveCorrectDefaultValues()
        {
            ExtractorOptions options = new();

            options.OptionAlias.Should().BeNull();
            options.OptionAliasPrefix.Should().Be("--");
            options.OptionPrefix.Should().Be("-");
            options.OptionValueSeparator.Should().Be(" ");
            options.ValueDelimiter.Should().Be("\"");
            options.CommandIdRegex.Should().Be("^[A-Za-z0-9_-]*$");
            options.Separator.Should().Be(" ");
        }
    }
}