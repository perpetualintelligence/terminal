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
    public class RouterOptionsTests
    {
        [Fact]
        public void RouterOptionsShouldHaveCorrectDefaultValues()
        {
            RouterOptions options = new();

            options.Caret.Should().Be(">");
            options.Timeout.Should().Be(25000);
            options.MaxLength.Should().Be(1024);
            options.MaxClients.Should().Be(5);
            options.StreamDelimiter.Should().Be(TerminalIdentifiers.StreamDelimiter);
            options.RouteDelay.Should().Be(50);
            options.DisableResponse.Should().BeFalse();
        }
    }
}
