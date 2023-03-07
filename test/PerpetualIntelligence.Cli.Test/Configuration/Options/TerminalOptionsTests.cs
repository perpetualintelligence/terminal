/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using PerpetualIntelligence.Test;
using PerpetualIntelligence.Test.Services;
using Xunit;

namespace PerpetualIntelligence.Cli.Configuration.Options
{
    public class TerminalOptionsTests : InitializerTests
    {
        public TerminalOptionsTests() : base(TestLogger.Create<TerminalOptionsTests>())
        {
        }

        [Fact]
        public void TerminalOptionsTests_ShouldHaveCorrectDefaultValues()
        {
            TerminalOptions options = new();
        }
    }
}