/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerpetualIntelligence.Cli.Commands;
using PerpetualIntelligence.Cli.Commands.Checkers;
using PerpetualIntelligence.Cli.Commands.Comparers;
using PerpetualIntelligence.Cli.Commands.Extractors;
using PerpetualIntelligence.Cli.Commands.Handlers;
using PerpetualIntelligence.Cli.Commands.Providers;
using PerpetualIntelligence.Cli.Commands.Publishers;
using PerpetualIntelligence.Cli.Commands.Routers;
using PerpetualIntelligence.Cli.Commands.Stores;
using PerpetualIntelligence.Cli.Configuration.Options;
using PerpetualIntelligence.Cli.Integration;
using PerpetualIntelligence.Cli.Mocks;
using PerpetualIntelligence.Protocols.Abstractions.Comparers;

using PerpetualIntelligence.Test;
using PerpetualIntelligence.Test.Services;
using System;
using System.Linq;

namespace PerpetualIntelligence.Cli.Extensions
{
    [TestClass]
    public class ICliBuilderExtensionsTests : InitializerTests
    {
        public ICliBuilderExtensionsTests() : base(TestLogger.Create<ICliBuilderExtensionsTests>())
        {
        }

        [TestMethod]
        public void AddAddStringComparerShouldCorrectlyInitialize()
        {
            cliBuilder.AddStringComparer(StringComparison.Ordinal);

            var comparer = cliBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(IStringComparer)));
            Assert.IsNotNull(comparer);
            Assert.AreEqual(ServiceLifetime.Transient, comparer.Lifetime);

            // This registers a factory so we build to check the instance
            var serviceProvider = cliBuilder.Services.BuildServiceProvider();
            var instance = serviceProvider.GetService<IStringComparer>();
            Assert.IsInstanceOfType(instance, typeof(StringComparisonComparer));
        }

        [TestMethod]
        public void AddArgumentCheckerShouldCorrectlyInitialize()
        {
            cliBuilder.AddArgumentChecker<MockArgumentMapper, MockArgumentChecker>();

            var arg = cliBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(IArgumentChecker)));
            Assert.IsNotNull(arg);
            Assert.AreEqual(ServiceLifetime.Transient, arg.Lifetime);
            Assert.AreEqual(typeof(MockArgumentChecker), arg.ImplementationType);
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
        public void AddCommandDescriptorMultipleTimeShouldError()
        {
            var cmd = new CommandDescriptor("id1", "name1", "prefix1", "desc");
            cliBuilder.AddDescriptor<MockCommandRunner, MockCommandChecker>(cmd);

            // Again
            TestHelper.AssertThrowsErrorException(() => cliBuilder.AddDescriptor<MockCommandRunner, MockCommandChecker>(cmd), Errors.InvalidConfiguration, "The command descriptor is already configured and added to the service collection.");
        }

        [TestMethod]
        public void AddCommandDescriptorShouldCorrectlyInitializeCheckerAndRunner()
        {
            cliBuilder.AddDescriptor<MockCommandRunner, MockCommandChecker>(new CommandDescriptor("id1", "name1", "prefix1", "desc"));

            var cmdDescriptor = cliBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(CommandDescriptor)));
            Assert.IsNotNull(cmdDescriptor);
            Assert.AreEqual(ServiceLifetime.Singleton, cmdDescriptor.Lifetime);
            CommandDescriptor? impIstance = (CommandDescriptor?)cmdDescriptor.ImplementationInstance;
            Assert.IsNotNull(impIstance);
            Assert.AreEqual("id1", impIstance.Id);
            Assert.AreEqual("name1", impIstance.Name);
            Assert.AreEqual("prefix1", impIstance.Prefix);
            Assert.AreEqual(typeof(MockCommandRunner), impIstance._runner);
            Assert.AreEqual(typeof(MockCommandChecker), impIstance._checker);
            Assert.IsFalse(impIstance.IsGroup);
            Assert.IsFalse(impIstance.IsRoot);

            var cmdRunner = cliBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(MockCommandRunner)));
            Assert.IsNotNull(cmdRunner);
            Assert.AreEqual(ServiceLifetime.Transient, cmdRunner.Lifetime);

            var cmdChecker = cliBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(MockCommandChecker)));
            Assert.IsNotNull(cmdChecker);
            Assert.AreEqual(ServiceLifetime.Transient, cmdChecker.Lifetime);
        }

        [TestMethod]
        public void AddCommandDescriptorStoreShouldCorrectlyInitialize()
        {
            cliBuilder.AddDescriptorStore<MockCommandDescriptorStore>();

            var serviceDescriptor = cliBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(ICommandDescriptorStore)));
            Assert.IsNotNull(serviceDescriptor);
            Assert.AreEqual(ServiceLifetime.Transient, serviceDescriptor.Lifetime);
            Assert.AreEqual(typeof(MockCommandDescriptorStore), serviceDescriptor.ImplementationType);
        }

        [TestMethod]
        public void AddCommandDescriptorWithGroupAndNoRootShouldNotError()
        {
            var cmd = new CommandDescriptor("id1", "name1", "prefix1", "desc");
            Assert.IsFalse(cmd.IsGroup);
            Assert.IsFalse(cmd.IsRoot);

            cliBuilder.AddDescriptor<MockCommandRunner, MockCommandChecker>(cmd, isGroup: true, isRoot: false);
            Assert.IsTrue(cmd.IsGroup);
            Assert.IsFalse(cmd.IsRoot);
        }

        [TestMethod]
        public void AddCommandDescriptorWithGroupAndRootShouldNotError()
        {
            var cmd = new CommandDescriptor("id1", "name1", "prefix1", "desc");
            Assert.IsFalse(cmd.IsGroup);
            Assert.IsFalse(cmd.IsRoot);

            cliBuilder.AddDescriptor<MockCommandRunner, MockCommandChecker>(cmd, isGroup: true, isRoot: true);
            Assert.IsTrue(cmd.IsGroup);
            Assert.IsTrue(cmd.IsRoot);
        }

        [TestMethod]
        public void AddCommandDescriptorWithRootAndNoGroupShouldError()
        {
            var cmd = new CommandDescriptor("id1", "name1", "prefix1", "desc");

            TestHelper.AssertThrowsErrorException(() => cliBuilder.AddDescriptor<MockCommandRunner, MockCommandChecker>(cmd, isGroup: false, isRoot: true), Errors.InvalidConfiguration, "The root command must also be a command group. command_id=id1 command_name=name1");
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
        public void AddExtractorWithDefaultArgValueProviderShouldCorrectlyInitialize()
        {
            cliBuilder.AddExtractor<MockCommandExtractor, MockArgumentExtractor, MockDefaultArgumentValueProvider>();

            var cmd = cliBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(ICommandExtractor)));
            Assert.IsNotNull(cmd);
            Assert.AreEqual(ServiceLifetime.Transient, cmd.Lifetime);
            Assert.AreEqual(typeof(MockCommandExtractor), cmd.ImplementationType);

            var arg = cliBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(IArgumentExtractor)));
            Assert.IsNotNull(arg);
            Assert.AreEqual(ServiceLifetime.Transient, arg.Lifetime);
            Assert.AreEqual(typeof(MockArgumentExtractor), arg.ImplementationType);

            var def = cliBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(IDefaultArgumentValueProvider)));
            Assert.IsNotNull(def);
            Assert.AreEqual(ServiceLifetime.Transient, def.Lifetime);
            Assert.AreEqual(typeof(MockDefaultArgumentValueProvider), def.ImplementationType);
        }

        [TestMethod]
        public void AddExtractorWithDefaultProviderShouldCorrectlyInitialize()
        {
            cliBuilder.AddExtractor<MockCommandExtractor, MockArgumentExtractor, MockDefaultArgumentProvider, MockDefaultArgumentValueProvider>();

            var cmd = cliBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(ICommandExtractor)));
            Assert.IsNotNull(cmd);
            Assert.AreEqual(ServiceLifetime.Transient, cmd.Lifetime);
            Assert.AreEqual(typeof(MockCommandExtractor), cmd.ImplementationType);

            var arg = cliBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(IArgumentExtractor)));
            Assert.IsNotNull(arg);
            Assert.AreEqual(ServiceLifetime.Transient, arg.Lifetime);
            Assert.AreEqual(typeof(MockArgumentExtractor), arg.ImplementationType);

            var def = cliBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(IDefaultArgumentProvider)));
            Assert.IsNotNull(def);
            Assert.AreEqual(ServiceLifetime.Transient, def.Lifetime);
            Assert.AreEqual(typeof(MockDefaultArgumentProvider), def.ImplementationType);

            var defValue = cliBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(IDefaultArgumentValueProvider)));
            Assert.IsNotNull(defValue);
            Assert.AreEqual(ServiceLifetime.Transient, defValue.Lifetime);
            Assert.AreEqual(typeof(MockDefaultArgumentValueProvider), defValue.ImplementationType);
        }

        [TestMethod]
        public void AddPublisherShouldCorrectlyInitialize()
        {
            cliBuilder.AddErrorPublisher<MockErrorPublisher, MockExceptionPublisher>();

            var err = cliBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(IErrorPublisher)));
            Assert.IsNotNull(err);
            Assert.AreEqual(ServiceLifetime.Transient, err.Lifetime);
            Assert.AreEqual(typeof(MockErrorPublisher), err.ImplementationType);

            var exe = cliBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(IExceptionPublisher)));
            Assert.IsNotNull(exe);
            Assert.AreEqual(ServiceLifetime.Transient, exe.Lifetime);
            Assert.AreEqual(typeof(MockExceptionPublisher), exe.ImplementationType);
        }

        [TestMethod]
        public void AddRoutingShouldCorrectlyInitialize()
        {
            cliBuilder.AddRouter<MockCommandRouter, MockCommandHandler>();

            var router = cliBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(ICommandRouter)));
            Assert.IsNotNull(router);
            Assert.AreEqual(ServiceLifetime.Transient, router.Lifetime);
            Assert.AreEqual(typeof(MockCommandRouter), router.ImplementationType);

            var handler = cliBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(ICommandHandler)));
            Assert.IsNotNull(handler);
            Assert.AreEqual(ServiceLifetime.Transient, handler.Lifetime);
            Assert.AreEqual(typeof(MockCommandHandler), handler.ImplementationType);
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
    }
}
