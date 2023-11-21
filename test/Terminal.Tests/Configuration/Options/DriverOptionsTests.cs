/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using PerpetualIntelligence.Test;
using PerpetualIntelligence.Test.Services;
using Xunit;

namespace PerpetualIntelligence.Terminal.Configuration.Options
{
    public class DriverOptionsTests : InitializerTests
    {
        public DriverOptionsTests() : base(TestLogger.Create<TerminalOptionsTests>())
        {
        }

        [Fact]
        public void TerminalOptionsShouldHaveCorrectDefaultValues()
        {
            DriverOptions options = new();

            options.Enabled.Should().BeNull();
            options.Name.Should().BeNull();
        }
    }
}