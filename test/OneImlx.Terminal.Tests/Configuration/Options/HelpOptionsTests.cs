/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Xunit;

namespace OneImlx.Terminal.Configuration.Options
{
    public class HelpOptionsTests
    {
        [Fact]
        public void ShouldHaveCorrectDefaultValues()
        {
            HelpOptions options = new();

            options.OptionAlias.Should().Be("h");
            options.OptionId.Should().Be("help");
            options.Disabled.Should().BeNull();
        }
    }
}