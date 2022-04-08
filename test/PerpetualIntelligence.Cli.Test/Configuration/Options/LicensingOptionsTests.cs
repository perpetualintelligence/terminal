/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerpetualIntelligence.Cli.Licensing;
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
            LicensingOptions options = new ();

            Assert.AreEqual(LicenseCheckMode.Online, options.CheckMode);
            Assert.IsNull(options.ConsumerTenantId);
            Assert.AreEqual(LicenseKeySource.JsonFile, options.KeySource);
            Assert.IsNull(options.ProviderTenantId);
            Assert.IsNull(options.Subject);
        }
    }
}
