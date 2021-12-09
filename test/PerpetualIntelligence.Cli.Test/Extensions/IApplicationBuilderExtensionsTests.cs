/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerpetualIntelligence.Test;
using PerpetualIntelligence.Test.Services;

namespace PerpetualIntelligence.Cli.Extensions
{
    [TestClass]
    public class IApplicationBuilderExtensionsTests : OneImlxLogTest
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
