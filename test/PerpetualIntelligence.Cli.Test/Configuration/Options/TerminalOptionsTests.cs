/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerpetualIntelligence.Test;
using PerpetualIntelligence.Test.Services;

namespace PerpetualIntelligence.Cli.Configuration.Options
{
    [TestClass]
    public class TerminalOptionsTests : InitializerTests
    {
        public TerminalOptionsTests() : base(TestLogger.Create<TerminalOptionsTests>())
        {
        }

        [TestMethod]
        public void TerminalOptionsTests_ShouldHaveCorrectDefaultValues()
        {
            TerminalOptions options = new();
            options.LoggerIndent.Should().Be(4);
            options.LogToStandard.Should().BeNull();
        }
    }
}