/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerpetualIntelligence.Test;
using PerpetualIntelligence.Test.Services;

namespace PerpetualIntelligence.Terminal.Configuration.Options
{
    [TestClass]
    public class TerminalOptionsTests : InitializerTests
    {
        public TerminalOptionsTests() : base(TestLogger.Create<TerminalOptionsTests>())
        {
        }

        [TestMethod]
        public void CliOptionsShouldHaveCorrectDefaultValues()
        {
            TerminalOptions options = new();

            Assert.IsNotNull(options.Authentication);
            Assert.IsNotNull(options.Checker);
            Assert.IsNotNull(options.Extractor);
            Assert.IsNotNull(options.Logging);
            Assert.IsNotNull(options.Handler);
            Assert.IsNotNull(options.Licensing);
            Assert.IsNotNull(options.Router);
            Assert.IsNotNull(options.Http);
            Assert.IsNotNull(options.Terminal);
            Assert.IsNotNull(options.Help);
        }
    }
}