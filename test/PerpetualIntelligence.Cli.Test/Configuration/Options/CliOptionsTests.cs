/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerpetualIntelligence.Test;
using PerpetualIntelligence.Test.Services;

namespace PerpetualIntelligence.Cli.Configuration.Options
{
    [TestClass]
    public class CliOptionsTests : InitializerTests
    {
        public CliOptionsTests() : base(TestLogger.Create<CliOptionsTests>())
        {
        }

        [TestMethod]
        public void CliOptionsShouldHaveCorrectDefaultValues()
        {
            CliOptions options = new();

            Assert.IsNotNull(options.Authentication);
            Assert.IsNotNull(options.Checker);
            Assert.IsNotNull(options.Extractor);
            Assert.IsNotNull(options.Logging);
            Assert.IsNotNull(options.Handler);
            Assert.IsNotNull(options.Licensing);
            Assert.IsNotNull(options.Router);
        }
    }
}
