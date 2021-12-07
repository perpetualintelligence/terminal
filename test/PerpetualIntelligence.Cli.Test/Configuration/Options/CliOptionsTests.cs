/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerpetualIntelligence.Test;
using PerpetualIntelligence.Test.Services;

namespace PerpetualIntelligence.Cli.Configuration.Options
{
    [TestClass]
    public class CliOptionsTests : OneImlxLogTest
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
        }
    }
}
