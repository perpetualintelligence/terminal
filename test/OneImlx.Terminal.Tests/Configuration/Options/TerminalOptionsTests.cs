/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Xunit;

namespace OneImlx.Terminal.Configuration.Options
{
    public class TerminalOptionsTests
    {
        [Fact]
        public void TerminalOptionsShouldHaveCorrectDefaultValues()
        {
            TerminalOptions options = new();

            options.Id.Should().BeEmpty();

            options.Driver.Should().NotBeNull();
            options.Authentication.Should().NotBeNull();
            options.Checker.Should().NotBeNull();
            options.Parser.Should().NotBeNull();
            options.Licensing.Should().NotBeNull();
            options.Router.Should().NotBeNull();
            options.Help.Should().NotBeNull();
            options.Dynamics.Should().NotBeNull();
        }
    }
}
