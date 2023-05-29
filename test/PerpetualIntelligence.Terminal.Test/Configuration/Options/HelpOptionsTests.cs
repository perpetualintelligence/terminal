/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using PerpetualIntelligence.Test;
using PerpetualIntelligence.Test.Services;
using Xunit;

namespace PerpetualIntelligence.Terminal.Configuration.Options
{
    public class HelpOptionsTests : InitializerTests
    {
        public HelpOptionsTests() : base(TestLogger.Create<HelpOptionsTests>())
        {
        }

        [Fact]
        public void ShouldHaveCorrectDefaultValues()
        {
            HelpOptions options = new();

            options.OptionAlias.Should().Be("H");
            options.OptionId.Should().Be("help");
            options.Disabled.Should().BeNull();
        }
    }
}