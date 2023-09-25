/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerpetualIntelligence.Test;
using PerpetualIntelligence.Test.Services;

namespace PerpetualIntelligence.Terminal.Configuration.Options
{
    [TestClass]
    public class CheckerOptionsTests : InitializerTests
    {
        public CheckerOptionsTests() : base(TestLogger.Create<CheckerOptionsTests>())
        {
        }

        [TestMethod]
        public void CheckerOptionsShouldHaveCorrectDefaultValues()
        {
            CheckerOptions options = new();

            Assert.AreEqual(null, options.AllowObsolete);
            Assert.AreEqual(null, options.StrictValueType);
        }
    }
}