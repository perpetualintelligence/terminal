/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerpetualIntelligence.Test;
using PerpetualIntelligence.Test.Services;

namespace PerpetualIntelligence.Cli.Extensions
{
    [TestClass]
    public class IApplicationBuilderExtensionsTests : LogTest
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
