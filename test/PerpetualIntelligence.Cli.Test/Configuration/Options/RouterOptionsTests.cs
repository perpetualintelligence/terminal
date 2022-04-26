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
    public class RouterOptionsTests : InitializerTests
    {
        public RouterOptionsTests() : base(TestLogger.Create<RouterOptionsTests>())
        {
        }

        [TestMethod]
        public void RouterOptionsShouldHaveCorrectDefaultValues()
        {
            RouterOptions options = new();

            Assert.AreEqual(25000, options.Timeout);
        }
    }
}
