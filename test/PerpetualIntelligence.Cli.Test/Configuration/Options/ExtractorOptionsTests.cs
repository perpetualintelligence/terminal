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
    public class ExtractorOptionsTests : LogTest
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
            Assert.AreEqual(false, options.DefaultArgumentValue);
        }
    }
}
