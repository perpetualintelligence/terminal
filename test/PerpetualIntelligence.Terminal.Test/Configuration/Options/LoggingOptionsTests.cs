/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using PerpetualIntelligence.Test;
using PerpetualIntelligence.Test.Services;
using Xunit;

namespace PerpetualIntelligence.Terminal.Configuration.Options
{
    public class LoggingOptionsTests : InitializerTests
    {
        public LoggingOptionsTests() : base(TestLogger.Create<LoggingOptionsTests>())
        {
        }

        [Fact]
        public void DefaultValuesShouldBeCorrect()
        {
            LoggingOptions options = new();

            options.LoggerIndent.Should().Be(4);
            options.LogToStandard.Should().BeNull();
        }

        [Fact]
        public void LoggingOptionsShouldHaveCorrectDefaultValues()
        {
            LoggingOptions options = new();

            options.ObsureInvalidOptions.Should().BeTrue();
            options.ObscureStringForInvalidOption.Should().Be("****");
        }
    }
}