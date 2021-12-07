/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerpetualIntelligence.Test;
using PerpetualIntelligence.Test.Services;

namespace PerpetualIntelligence.Cli.Integration.Configuration.Options
{
    [TestClass]
    public class ExtractorOptionsTests : OneImlxLogTest
    {
        public ExtractorOptionsTests() : base(TestLogger.Create<ExtractorOptionsTests>())
        {
        }

        [TestMethod]
        public void ExtractorOptionsShouldHaveCorrectDefaultValues()
        {
            ExtractorOptions options = new ExtractorOptions();

            Assert.AreEqual("-", options.ArgumentPrefix);
            Assert.AreEqual("=", options.ArgumentSeparator);
            Assert.AreEqual(" ", options.Separator);
        }
    }
}
