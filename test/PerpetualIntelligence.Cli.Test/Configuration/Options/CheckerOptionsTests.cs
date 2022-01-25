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
    public class CheckerOptionsTests : LogTest
    {
        public CheckerOptionsTests() : base(TestLogger.Create<CheckerOptionsTests>())
        {
        }

        [TestMethod]
        public void CheckerOptionsShouldHaveCorrectDefaultValues()
        {
            CheckerOptions options = new CheckerOptions();

            Assert.AreEqual(null, options.AllowObsoleteArgument);
        }
    }
}
