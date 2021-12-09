/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerpetualIntelligence.Test;
using PerpetualIntelligence.Test.Services;
using System;

namespace PerpetualIntelligence.Cli.Integration
{
    [TestClass]
    public class CliBuilderTests : OneImlxLogTest
    {
        public CliBuilderTests() : base(TestLogger.Create<CliBuilderTests>())
        {
        }

        [TestMethod]
        public void CliBuilderShouldReturnIserviceCollection()
        {
            CliBuilder cliBuilder = new (serviceCollection);
            Assert.AreEqual(serviceCollection, cliBuilder.Services);
        }

        protected override void OnTestCleanup()
        {
            host.Dispose();
        }

        protected override void OnTestInitialize()
        {
            var hostBuilder = Host.CreateDefaultBuilder(Array.Empty<string>()).ConfigureServices(ConfigureServicesDelegate);
            host = hostBuilder.Build();
        }

        private void ConfigureServicesDelegate(IServiceCollection arg2)
        {
            serviceCollection = arg2;
        }

        private IHost host = null!;
        private IServiceCollection serviceCollection = null!;
    }
}
