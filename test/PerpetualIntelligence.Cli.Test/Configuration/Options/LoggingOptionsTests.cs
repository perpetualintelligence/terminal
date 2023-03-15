/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using FluentAssertions;
using PerpetualIntelligence.Test;
using PerpetualIntelligence.Test.Services;
using Xunit;

namespace PerpetualIntelligence.Cli.Configuration.Options
{
    public class LoggingOptionsTests : InitializerTests
    {
        public LoggingOptionsTests() : base(TestLogger.Create<LoggingOptionsTests>())
        {
        }

        [Fact]
        public void DefualtValuesShouldBeCorrect()
        {
            LoggingOptions options = new LoggingOptions();

            options.LoggerIndent.Should().Be(4);
            options.LogToStandard.Should().BeNull();
        }

        [Fact]
        public void LoggingOptionsShouldHaveCorrectDefaultValues()
        {
            LoggingOptions options = new LoggingOptions();

            options.ObsureInvalidOptions.Should().BeTrue();
            options.ObscureStringForInvalidOption.Should().Be("****");
        }
    }
}