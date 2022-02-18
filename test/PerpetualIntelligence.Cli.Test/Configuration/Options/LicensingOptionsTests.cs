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
    public class LicensingOptionsTests : InitializerTests
    {
        public LicensingOptionsTests() : base(TestLogger.Create<LoggingOptionsTests>())
        {
        }

        [TestMethod]
        public void LicensingOptionsTestsShouldHaveCorrectDefaultValues()
        {
            LicensingOptions options = new LicensingOptions();

            Assert.IsNull(options.LicenseKeys);
            Assert.IsNull(options.SigningKey);
            Assert.AreEqual(LicenseKeyType.PublicToken, options.LicenseKeyType);
        }
    }
}
