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
    public class ExtractorOptionsTests : InitializerTests
    {
        public ExtractorOptionsTests() : base(TestLogger.Create<ExtractorOptionsTests>())
        {
        }

        [TestMethod]
        public void ExtractorOptionsShouldHaveCorrectDefaultValues()
        {
            ExtractorOptions options = new();

            Assert.IsNull(options.ArgumentAlias);
            Assert.AreEqual("-", options.ArgumentAliasPrefix);
            Assert.AreEqual("-", options.ArgumentPrefix);
            Assert.AreEqual("=", options.ArgumentSeparator);
            Assert.IsNull(options.ArgumentValueWithIn);
            Assert.AreEqual("^[A-Za-z0-9_-]*$", options.CommandIdRegexPattern);
            Assert.IsNull(options.DefaultArgumentValue);
            Assert.IsNull(options.DefaultArgument);
            Assert.AreEqual(" ", options.Separator);
        }
    }
}
