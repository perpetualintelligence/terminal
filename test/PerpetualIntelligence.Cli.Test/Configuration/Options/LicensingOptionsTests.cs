/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerpetualIntelligence.Protocols.Licensing;
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
            LicensingOptions options = new();

            Assert.IsNull(options.AuthorizedApplicationId);
            Assert.IsNull(options.ConsumerTenantId);
            Assert.AreEqual(SaaSKeySources.JsonFile, options.KeySource);
            Assert.IsNull(options.LicenseKey);
            Assert.AreEqual(SaaSProviders.PerpetualIntelligence, options.ProviderId);
            Assert.IsNull(options.Subject);
        }
    }
}
