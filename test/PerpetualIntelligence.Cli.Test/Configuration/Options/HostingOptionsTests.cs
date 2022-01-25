/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved
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
    public class HostingOptionsTests : LogTest
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
