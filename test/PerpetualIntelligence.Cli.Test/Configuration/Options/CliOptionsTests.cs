/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerpetualIntelligence.Test;
using PerpetualIntelligence.Test.Services;

namespace PerpetualIntelligence.Cli.Configuration.Options
{
    [TestClass]
    public class CliOptionsTests : LogTest
    {
        public CliOptionsTests() : base(TestLogger.Create<CliOptionsTests>())
        {
        }

        [TestMethod]
        public void CliOptionsShouldHaveCorrectDefaultValues()
        {
            CliOptions options = new CliOptions();

            Assert.IsNotNull(options.Checker);
            Assert.IsNotNull(options.Extractor);
            Assert.IsNotNull(options.Logging);
            Assert.IsNotNull(options.Hosting);
        }
    }
}
