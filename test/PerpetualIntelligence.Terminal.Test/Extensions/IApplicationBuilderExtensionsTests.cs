/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerpetualIntelligence.Test;
using PerpetualIntelligence.Test.Services;

namespace PerpetualIntelligence.Cli.Extensions
{
    [TestClass]
    public class IApplicationBuilderExtensionsTests : InitializerTests
    {
        public IApplicationBuilderExtensionsTests() : base(TestLogger.Create<IApplicationBuilderExtensionsTests>())
        {
        }

        [TestMethod]
        public void UseCliShouldInitializeCorrectly()
        {
            // TODO
        }
    }
}
