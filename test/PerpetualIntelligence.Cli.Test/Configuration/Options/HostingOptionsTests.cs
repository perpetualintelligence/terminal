/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerpetualIntelligence.Test;
using PerpetualIntelligence.Test.Services;
using System.Threading;

namespace PerpetualIntelligence.Cli.Configuration.Options
{
    [TestClass]
    public class HostingOptionsTests : OneImlxLogTest
    {
        public HostingOptionsTests() : base(TestLogger.Create<HostingOptionsTests>())
        {
        }

        [TestMethod]
        public void LoggingOptionsShouldHaveCorrectDefaultValues()
        {
            HostingOptions options = new();

            Assert.AreEqual(10000, options.CommandRouterTimeout);
        }
    }
}
