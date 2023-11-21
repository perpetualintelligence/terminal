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
    public class TerminalOptionsTests : InitializerTests
    {
        public TerminalOptionsTests() : base(TestLogger.Create<TerminalOptionsTests>())
        {
        }

        [Fact]
        public void TerminalOptionsShouldHaveCorrectDefaultValues()
        {
            TerminalOptions options = new();

            options.Id.Should().BeEmpty();

            options.Driver.Should().NotBeNull();
            options.Authentication.Should().NotBeNull();
            options.Checker.Should().NotBeNull();
            options.Parser.Should().NotBeNull();
            options.Handler.Should().NotBeNull();
            options.Licensing.Should().NotBeNull();
            options.Router.Should().NotBeNull();
            options.Http.Should().NotBeNull();
            options.Help.Should().NotBeNull();
        }
    }
}