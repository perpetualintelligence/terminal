/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerpetualIntelligence.Shared.Licensing;
using PerpetualIntelligence.Test;
using PerpetualIntelligence.Test.Services;

namespace PerpetualIntelligence.Terminal.Configuration.Options
{
    [TestClass]
    public class LicensingOptionsTests : InitializerTests
    {
        public LicensingOptionsTests() : base(TestLogger.Create<LicensingOptionsTests>())
        {
        }

        [TestMethod]
        public void LicensingOptionsTestsShouldHaveCorrectDefaultValues()
        {
            LicensingOptions options = new();

            Assert.IsNull(options.AuthorizedApplicationId);
            Assert.IsNull(options.ConsumerTenantId);
            Assert.AreEqual(LicenseSources.JsonFile, options.KeySource);
            Assert.IsNull(options.LicenseKey);
            Assert.AreEqual(LicenseProviders.PerpetualIntelligence, options.ProviderId);
            Assert.IsNull(options.Subject);
        }
    }
}