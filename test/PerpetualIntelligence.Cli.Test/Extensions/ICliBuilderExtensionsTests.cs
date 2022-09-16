/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerpetualIntelligence.Cli.Commands;
using PerpetualIntelligence.Cli.Commands.Checkers;
using PerpetualIntelligence.Cli.Commands.Extractors;
using PerpetualIntelligence.Cli.Commands.Handlers;
using PerpetualIntelligence.Cli.Commands.Providers;
using PerpetualIntelligence.Cli.Commands.Routers;
using PerpetualIntelligence.Cli.Configuration.Options;
using PerpetualIntelligence.Cli.Integration;
using PerpetualIntelligence.Cli.Mocks;
using PerpetualIntelligence.Cli.Stores;

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
        public void AddCommandDescriptorMultipleTimeShouldNotError()
        {
            cliBuilder.DefineCommand<MockCommandChecker, MockCommandRunner>("id1", "name1", "prefix1", "desc").Add();
            cliBuilder.DefineCommand<MockCommandChecker, MockCommandRunner>("id1", "name1", "prefix1", "desc").Add();

            var sp = cliBuilder.Services.BuildServiceProvider();
            var cmds = sp.GetServices<CommandDescriptor>();
            cmds.Count().Should().Be(2);

            cmds.First().Id.Equals("id1");
            cmds.Last().Id.Equals("id1");
        }

        [TestMethod]
        public void AddCommandDescriptorShouldCorrectlyInitializeCheckerAndRunner()
        {
            cliBuilder.DefineCommand<MockCommandChecker, MockCommandRunner>("id1", "name1", "prefix1", "desc").Add();

            var cmdDescriptor = cliBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(CommandDescriptor)));
            Assert.IsNotNull(cmdDescriptor);
            Assert.AreEqual(ServiceLifetime.Singleton, cmdDescriptor.Lifetime);
            CommandDescriptor? impIstance = (CommandDescriptor?)cmdDescriptor.ImplementationInstance;
            Assert.IsNotNull(impIstance);
            Assert.AreEqual("id1", impIstance.Id);
            Assert.AreEqual("name1", impIstance.Name);
            Assert.AreEqual("prefix1", impIstance.Prefix);
            Assert.AreEqual(typeof(MockCommandRunner), impIstance.Runner);
            Assert.AreEqual(typeof(MockCommandChecker), impIstance.Checker);
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
            cliBuilder.AddStoreHandler<MockCommandDescriptorStore>();

            var serviceDescriptor = cliBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(ICommandStoreHandler)));
            Assert.IsNotNull(serviceDescriptor);
            Assert.AreEqual(ServiceLifetime.Transient, serviceDescriptor.Lifetime);
            Assert.AreEqual(typeof(MockCommandDescriptorStore), serviceDescriptor.ImplementationType);
        }

        [TestMethod]
        public void AddCommandDescriptorWithGroupAndNoRootShouldNotError()
        {
            cliBuilder.DefineCommand<MockCommandChecker, MockCommandRunner>("id1", "name1", "prefix1", "desc", isGroup: true, isRoot: false).Add();

            IServiceProvider serviceProvider = cliBuilder.Services.BuildServiceProvider();
            CommandDescriptor cmd = serviceProvider.GetRequiredService<CommandDescriptor>();

            Assert.AreEqual("id1", cmd.Id);
            Assert.IsTrue(cmd.IsGroup);
            Assert.IsFalse(cmd.IsRoot);
            Assert.IsFalse(cmd.IsProtected);
        }

        [TestMethod]
        public void AddCommandDescriptorWithRootAndNoGroupShouldError()
        {
            TestHelper.AssertThrowsErrorException(() => cliBuilder.DefineCommand<MockCommandChecker, MockCommandRunner>("id1", "name1", "prefix1", "desc", isGroup: false, isRoot: true).Add(), Errors.InvalidConfiguration, "The root command must also be a grouped command. command_id=id1 command_name=name1");
        }

        [TestMethod]
        public void AddCommandDescriptorWithSpecialAnnotationsShouldNotError()
        {
            cliBuilder.DefineCommand<MockCommandChecker, MockCommandRunner>("id1", "name1", "prefix1", "desc", isGroup: true, isRoot: true, isProtected: true).Add();

            IServiceProvider serviceProvider = cliBuilder.Services.BuildServiceProvider();
            CommandDescriptor cmd = serviceProvider.GetRequiredService<CommandDescriptor>();

            Assert.IsTrue(cmd.IsGroup);
            Assert.IsTrue(cmd.IsRoot);
            Assert.IsTrue(cmd.IsProtected);
        }

        [TestMethod]
        public void AddCommandShouldCorrectlyInitialize()
        {
            ICommandBuilder commandBuilder = cliBuilder.DefineCommand<MockCommandChecker, MockCommandRunner>("id1", "name1", "prefix1", "description1");

            // AddCommand does not add ICommandBuilder to service collection.
            var servicesCmdBuilder = cliBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(ICommandBuilder)));
            Assert.IsNull(servicesCmdBuilder);

            // Command builder creates a new local service collection
            Assert.AreNotEqual(cliBuilder.Services, commandBuilder.Services);

            // Lifetime of command builder service
            var serviceDescriptor = commandBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(CommandDescriptor)));
            Assert.IsNotNull(serviceDescriptor);
            Assert.AreEqual(ServiceLifetime.Singleton, serviceDescriptor.Lifetime);

            // Check the instance
            var serviceProvider = commandBuilder.Services.BuildServiceProvider();
            var instance = serviceProvider.GetRequiredService<CommandDescriptor>();
            Assert.AreEqual("id1", instance.Id);
            Assert.AreEqual("name1", instance.Name);
            Assert.AreEqual("prefix1", instance.Prefix);
            Assert.AreEqual("description1", instance.Description);
            Assert.AreEqual(typeof(MockCommandChecker), instance.Checker);
            Assert.AreEqual(typeof(MockCommandRunner), instance.Runner);
            Assert.IsFalse(instance.IsGroup);
            Assert.IsFalse(instance.IsRoot);
            Assert.IsFalse(instance.IsProtected);
        }

        [TestMethod]
        public void AddCommandSpecialAnnotationsShouldCorrectlyInitialize()
        {
            ICommandBuilder commandBuilder = cliBuilder.DefineCommand<MockCommandChecker, MockCommandRunner>("id1", "name1", "prefix1", "description1", isGroup: true, isRoot: true, isProtected: true);

            // AddCommand does not add ICommandBuilder to service collection.
            var servicesCmdBuilder = cliBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(ICommandBuilder)));
            Assert.IsNull(servicesCmdBuilder);

            // Command builder creates a new local service collection
            Assert.AreNotEqual(cliBuilder.Services, commandBuilder.Services);

            // Lifetime of command builder service
            var serviceDescriptor = commandBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(CommandDescriptor)));
            Assert.IsNotNull(serviceDescriptor);
            Assert.AreEqual(ServiceLifetime.Singleton, serviceDescriptor.Lifetime);

            // Check the instance
            var serviceProvider = commandBuilder.Services.BuildServiceProvider();
            var instance = serviceProvider.GetRequiredService<CommandDescriptor>();
            Assert.AreEqual("id1", instance.Id);
            Assert.AreEqual("name1", instance.Name);
            Assert.AreEqual("prefix1", instance.Prefix);
            Assert.AreEqual("description1", instance.Description);
            Assert.AreEqual(typeof(MockCommandChecker), instance.Checker);
            Assert.AreEqual(typeof(MockCommandRunner), instance.Runner);
            Assert.IsTrue(instance.IsGroup);
            Assert.IsTrue(instance.IsRoot);
            Assert.IsTrue(instance.IsProtected);
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
            cliBuilder.AddErrorHandler<MockErrorPublisher, MockExceptionPublisher>();

            var err = cliBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(IErrorHandler)));
            Assert.IsNotNull(err);
            Assert.AreEqual(ServiceLifetime.Transient, err.Lifetime);
            Assert.AreEqual(typeof(MockErrorPublisher), err.ImplementationType);

            var exe = cliBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(IExceptionHandler)));
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

        [TestMethod]
        public void AddTextHandlerShouldCorrectlyInitialize()
        {
            cliBuilder.AddTextHandler<UnicodeTextHandler>();

            var comparer = cliBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(ITextHandler)));
            Assert.IsNotNull(comparer);
            Assert.AreEqual(ServiceLifetime.Transient, comparer.Lifetime);

            // This registers a factory so we build to check the instance
            var serviceProvider = cliBuilder.Services.BuildServiceProvider();
            var instance = serviceProvider.GetService<ITextHandler>();
            Assert.IsInstanceOfType(instance, typeof(UnicodeTextHandler));
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
