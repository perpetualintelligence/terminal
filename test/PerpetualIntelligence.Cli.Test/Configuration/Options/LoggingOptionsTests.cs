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
    public class LoggingOptionsTests : OneImlxLogTest
    {
        public LoggingOptionsTests() : base(TestLogger.Create<LoggingOptionsTests>())
        {
        }

        [TestMethod]
        public void LoggingOptionsShouldHaveCorrectDefaultValues()
        {
            LoggingOptions options = new LoggingOptions();

            Assert.AreEqual(null, options.ErrorArguments);
        }
    }
}
