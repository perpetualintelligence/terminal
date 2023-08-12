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
    public class ExtractorOptionsTests : InitializerTests
    {
        public ExtractorOptionsTests() : base(TestLogger.Create<ExtractorOptionsTests>())
        {
        }

        [TestMethod]
        public void ExtractorOptionsShouldHaveCorrectDefaultValues()
        {
            ExtractorOptions options = new();

            Assert.IsNull(options.OptionAlias);
            Assert.AreEqual("--", options.OptionAliasPrefix);
            Assert.AreEqual("-", options.OptionPrefix);
            Assert.AreEqual(" ", options.OptionValueSeparator);
            Assert.IsNull(options.OptionValueWithIn);
            Assert.AreEqual("^[A-Za-z0-9_-]*$", options.CommandIdRegex);
            Assert.AreEqual(" ", options.Separator);
        }
    }
}