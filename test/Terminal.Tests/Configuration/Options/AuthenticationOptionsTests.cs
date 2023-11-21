/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerpetualIntelligence.Test;
using PerpetualIntelligence.Test.Services;

namespace PerpetualIntelligence.Terminal.Configuration.Options
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
            Assert.AreEqual("http://localhost", options.RedirectUri);
            Assert.IsNull(options.Scopes);
            Assert.IsFalse(options.UseEmbeddedView.GetValueOrDefault());
        }
    }
}
