/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerpetualIntelligence.Test;
using PerpetualIntelligence.Test.Services;
using System;

namespace PerpetualIntelligence.Cli.Extensions
{
    [TestClass]
    public class IHostExtensionsTests : OneImlxLogTest
    {
        public IHostExtensionsTests() : base(TestLogger.Create<IHostExtensionsTests>())
        {
        }

        [TestMethod]
        public void RunRoutingTimeOutShoudFail()
        {
            // TODO
        }

        protected override void OnTestInitialize()
        {
            var hostBuilder = Host.CreateDefaultBuilder(Array.Empty<string>());
            host = hostBuilder.Build();
        }

        private IHost host = null!;
    }
}
