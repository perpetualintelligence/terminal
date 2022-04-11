/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerpetualIntelligence.Cli.Licensing;
using PerpetualIntelligence.Protocols.Licensing;
using PerpetualIntelligence.Test;
using PerpetualIntelligence.Test.Services;
using System;

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

            Assert.AreEqual(SaaSCheckModes.Online, options.CheckMode);
            Assert.IsNull(options.ConsumerTenantId);
            Assert.IsNull(options.HttpClientName);
            Assert.AreEqual(LicenseKeySource.JsonFile, options.KeySource);
            Assert.IsNull(options.LicenseKey);
            Assert.IsNull(options.ProviderId);
            Assert.IsNull(options.Subject);
        }
    }
}
