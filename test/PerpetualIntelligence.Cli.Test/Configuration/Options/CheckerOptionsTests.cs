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
    public class CheckerOptionsTests : OneImlxLogTest
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
