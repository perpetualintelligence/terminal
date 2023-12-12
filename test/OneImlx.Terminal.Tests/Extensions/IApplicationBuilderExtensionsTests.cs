/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using OneImlx.Test;
using OneImlx.Test.Services;

namespace OneImlx.Terminal.Extensions
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
