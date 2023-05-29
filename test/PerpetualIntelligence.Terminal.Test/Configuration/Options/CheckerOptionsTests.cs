/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerpetualIntelligence.Test;
using PerpetualIntelligence.Test.Services;

namespace PerpetualIntelligence.Cli.Configuration.Options
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
            CheckerOptions options = new ();

            Assert.AreEqual(null, options.AllowObsoleteOption);
            Assert.AreEqual(null, options.StrictOptionValueType);            
        }
    }
}
