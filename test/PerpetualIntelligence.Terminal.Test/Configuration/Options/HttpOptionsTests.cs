/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerpetualIntelligence.Test;
using PerpetualIntelligence.Test.Services;

namespace PerpetualIntelligence.Terminal.Configuration.Options
{
    [TestClass]
    public class HttpOptionsTests : InitializerTests
    {
        public HttpOptionsTests() : base(TestLogger.Create<HttpOptionsTests>())
        {
        }

        [TestMethod]
        public void HttpOptionsTests_ShouldHaveCorrectDefaultValues()
        {
            HttpOptions options = new();

            options.HttpClientName.Should().BeNull();
        }
    }
}
