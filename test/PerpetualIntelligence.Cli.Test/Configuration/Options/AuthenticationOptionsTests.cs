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
    public class AuthenticationOptionsTests : InitializerTests
    {
        public AuthenticationOptionsTests() : base(TestLogger.Create<AuthenticationOptionsTests>())
        {
        }

        [TestMethod]
        public void AuthenticationOptionsTestsTestsShouldHaveCorrectDefaultValues()
        {
            AuthenticationOptions options = new();

            Assert.IsNull(options.Authority);
            Assert.IsNull(options.ApplicationId);
            Assert.IsNull(options.TenantId);
            Assert.IsNull(options.HttpClientName);
            Assert.IsNull(options.Scopes);
        }
    }
}
