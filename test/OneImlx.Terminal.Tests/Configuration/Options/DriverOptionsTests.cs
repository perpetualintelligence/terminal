/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Xunit;

namespace OneImlx.Terminal.Configuration.Options
{
    public class DriverOptionsTests
    {
        [Fact]
        public void TerminalOptionsShouldHaveCorrectDefaultValues()
        {
            DriverOptions options = new();

            options.Enabled.Should().BeNull();
            options.Name.Should().BeNull();
        }
    }
}