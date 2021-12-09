/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerpetualIntelligence.Cli.Commands;
using PerpetualIntelligence.Cli.Commands.Checkers;
using PerpetualIntelligence.Cli.Commands.Extractors;
using PerpetualIntelligence.Cli.Commands.Handlers;
using PerpetualIntelligence.Cli.Commands.Routers;
using PerpetualIntelligence.Cli.Commands.Runners;
using PerpetualIntelligence.Cli.Configuration.Options;
using PerpetualIntelligence.Cli.Integration;
using PerpetualIntelligence.Cli.Mocks;
using PerpetualIntelligence.Test;
using PerpetualIntelligence.Test.Services;
using System;
using System.Linq;

namespace PerpetualIntelligence.Cli.Extensions
{
    [TestClass]
    public class ICliBuilderExtensionsTests : OneImlxLogTest
    {
        public ICliBuilderExtensionsTests() : base(TestLogger.Create<ICliBuilderExtensionsTests>())
        {
        }

        [TestMethod]
        public void AddCliOptionsShouldCorrectlyInitialize()
        {
            cliBuilder.AddCliOptions();

            var serviceDescriptor = cliBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(CliOptions)));
            Assert.IsNotNull(serviceDescriptor);
            Assert.AreEqual(ServiceLifetime.Singleton, serviceDescriptor.Lifetime);
        }

        [TestMethod]
        public void AddCommandIdentityShouldCorrectlyInitialize()
        {
            cliBuilder.AddCommandIdentity<MockCommandRunner, MockCommandChecker>(new CommandIdentity("id1", "name1", "prefix1"));

            var cmdDescriptor = cliBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(CommandIdentity)));
            Assert.IsNotNull(cmdDescriptor);
            Assert.AreEqual(ServiceLifetime.Singleton, cmdDescriptor.Lifetime);
            CommandIdentity? impIstance = (CommandIdentity?)cmdDescriptor.ImplementationInstance;
            Assert.IsNotNull(impIstance);
            Assert.AreEqual("id1", impIstance.Id);
            Assert.AreEqual("name1", impIstance.Name);
            Assert.AreEqual("prefix1", impIstance.Prefix);
            Assert.AreEqual(typeof(MockCommandRunner), impIstance.Runner);
            Assert.AreEqual(typeof(MockCommandChecker), impIstance.Checker);

            var cmdRunner = cliBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(ICommandRunner)));
            Assert.IsNotNull(cmdRunner);
            Assert.AreEqual(ServiceLifetime.Transient, cmdRunner.Lifetime);
            Assert.AreEqual(typeof(MockCommandRunner), cmdRunner.ImplementationType);

            var cmdChecker = cliBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(ICommandChecker)));
            Assert.IsNotNull(cmdChecker);
            Assert.AreEqual(ServiceLifetime.Transient, cmdChecker.Lifetime);
            Assert.AreEqual(typeof(MockCommandChecker), cmdChecker.ImplementationType);
        }

        [TestMethod]
        public void AddCommandIdentityStoreShouldCorrectlyInitialize()
        {
            cliBuilder.AddCommandIdentityStore<MockCommandIdentityStore>();

            var serviceDescriptor = cliBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(ICommandIdentityStore)));
            Assert.IsNotNull(serviceDescriptor);
            Assert.AreEqual(ServiceLifetime.Transient, serviceDescriptor.Lifetime);
            Assert.AreEqual(typeof(MockCommandIdentityStore), serviceDescriptor.ImplementationType);
        }

        [TestMethod]
        public void AddExtractorShouldCorrectlyInitialize()
        {
            cliBuilder.AddExtractor<MockCommandExtractor, MockArgumentExtractor>();

            var cmd = cliBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(ICommandExtractor)));
            Assert.IsNotNull(cmd);
            Assert.AreEqual(ServiceLifetime.Transient, cmd.Lifetime);
            Assert.AreEqual(typeof(MockCommandExtractor), cmd.ImplementationType);

            var arg = cliBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(IArgumentExtractor)));
            Assert.IsNotNull(arg);
            Assert.AreEqual(ServiceLifetime.Transient, arg.Lifetime);
            Assert.AreEqual(typeof(MockArgumentExtractor), arg.ImplementationType);
        }

        [TestMethod]
        public void AddRoutingShouldCorrectlyInitialize()
        {
            cliBuilder.AddRouting<MockCommandRouter, MockCommandHandler>();

            var router = cliBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(ICommandRouter)));
            Assert.IsNotNull(router);
            Assert.AreEqual(ServiceLifetime.Transient, router.Lifetime);
            Assert.AreEqual(typeof(MockCommandRouter), router.ImplementationType);

            var handler = cliBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(ICommandHandler)));
            Assert.IsNotNull(handler);
            Assert.AreEqual(ServiceLifetime.Transient, handler.Lifetime);
            Assert.AreEqual(typeof(MockCommandHandler), handler.ImplementationType);
        }

        [TestMethod]
        public void RunRoutingTimeOutShoudFail()
        {
            // TODO
        }

        protected override void OnTestInitialize()
        {
            IServiceCollection? serviceDescriptors = null;

            using var host = Host.CreateDefaultBuilder(Array.Empty<string>()).ConfigureServices(arg =>
            {
                serviceDescriptors = arg;
            }).Build();

            Assert.IsNotNull(serviceDescriptors);
            cliBuilder = serviceDescriptors.AddCliBuilder();
        }

        private ICliBuilder cliBuilder = null!;
        private IHost host = null!;
    }
}
