/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerpetualIntelligence.Terminal.Commands;
using PerpetualIntelligence.Terminal.Commands.Checkers;
using PerpetualIntelligence.Terminal.Commands.Extractors;
using PerpetualIntelligence.Terminal.Commands.Handlers;
using PerpetualIntelligence.Terminal.Commands.Providers;
using PerpetualIntelligence.Terminal.Commands.Routers;
using PerpetualIntelligence.Terminal.Configuration.Options;
using PerpetualIntelligence.Terminal.Events;
using PerpetualIntelligence.Terminal.Hosting;
using PerpetualIntelligence.Terminal.Mocks;
using PerpetualIntelligence.Terminal.Stores;

using PerpetualIntelligence.Test;
using PerpetualIntelligence.Test.Services;
using System;
using System.Linq;

namespace PerpetualIntelligence.Terminal.Extensions
{
    [TestClass]
    public class ITerminalBuilderExtensionsTests : InitializerTests
    {
        public ITerminalBuilderExtensionsTests() : base(TestLogger.Create<ITerminalBuilderExtensionsTests>())
        {
        }

        [TestMethod]
        public void AddHelpProviderShouldCorrectlyInitialize()
        {
            terminalBuilder.AddHelpProvider<MockHelpProvider>();

            var arg = terminalBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(IHelpProvider)));
            Assert.IsNotNull(arg);
            Assert.AreEqual(ServiceLifetime.Singleton, arg.Lifetime);
            Assert.AreEqual(typeof(MockHelpProvider), arg.ImplementationType);
        }

        [TestMethod]
        public void AddEventHandlerShouldCorrectlyInitialize()
        {
            terminalBuilder.AddEventHandler<MockAsyncEventHandler>();

            var arg = terminalBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(IAsyncEventHandler)));
            Assert.IsNotNull(arg);
            Assert.AreEqual(ServiceLifetime.Transient, arg.Lifetime);
            Assert.AreEqual(typeof(MockAsyncEventHandler), arg.ImplementationType);
        }

        [TestMethod]
        public void AddArgumentCheckerShouldCorrectlyInitialize()
        {
            terminalBuilder.AddOptionChecker<MockArgumentMapper, MockArgumentChecker>();

            var arg = terminalBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(IOptionChecker)));
            Assert.IsNotNull(arg);
            Assert.AreEqual(ServiceLifetime.Transient, arg.Lifetime);
            Assert.AreEqual(typeof(MockArgumentChecker), arg.ImplementationType);
        }

        [TestMethod]
        public void AddTerminalOptionsShouldCorrectlyInitialize()
        {
            terminalBuilder.AddTerminalOptions();

            var serviceDescriptor = terminalBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(TerminalOptions)));
            Assert.IsNotNull(serviceDescriptor);
            Assert.AreEqual(ServiceLifetime.Singleton, serviceDescriptor.Lifetime);
        }

        [TestMethod]
        public void AddCommandDescriptorMultipleTimeShouldNotError()
        {
            terminalBuilder.DefineCommand<MockCommandChecker, MockCommandRunner>("id1", "name1", "prefix1", "desc").Add();
            terminalBuilder.DefineCommand<MockCommandChecker, MockCommandRunner>("id1", "name1", "prefix1", "desc").Add();

            var sp = terminalBuilder.Services.BuildServiceProvider();
            var cmds = sp.GetServices<CommandDescriptor>();
            cmds.Count().Should().Be(2);

            cmds.First().Id.Equals("id1");
            cmds.Last().Id.Equals("id1");
        }

        [TestMethod]
        public void AddCommandDescriptorShouldCorrectlyInitializeCheckerAndRunner()
        {
            terminalBuilder.DefineCommand<MockCommandChecker, MockCommandRunner>("id1", "name1", "prefix1", "desc").Add();

            var cmdDescriptor = terminalBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(CommandDescriptor)));
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

            var cmdRunner = terminalBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(MockCommandRunner)));
            Assert.IsNotNull(cmdRunner);
            Assert.AreEqual(ServiceLifetime.Transient, cmdRunner.Lifetime);

            var cmdChecker = terminalBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(MockCommandChecker)));
            Assert.IsNotNull(cmdChecker);
            Assert.AreEqual(ServiceLifetime.Transient, cmdChecker.Lifetime);
        }

        [TestMethod]
        public void AddCommandDescriptorStoreShouldCorrectlyInitialize()
        {
            terminalBuilder.AddStoreHandler<MockCommandDescriptorStore>();

            var serviceDescriptor = terminalBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(ICommandStoreHandler)));
            Assert.IsNotNull(serviceDescriptor);
            Assert.AreEqual(ServiceLifetime.Transient, serviceDescriptor.Lifetime);
            Assert.AreEqual(typeof(MockCommandDescriptorStore), serviceDescriptor.ImplementationType);
        }

        [TestMethod]
        public void AddCommandDescriptorWithGroupAndNoRootShouldNotError()
        {
            terminalBuilder.DefineCommand<MockCommandChecker, MockCommandRunner>("id1", "name1", "prefix1", "desc", isRoot: false, isGroup: true).Add();

            IServiceProvider serviceProvider = terminalBuilder.Services.BuildServiceProvider();
            CommandDescriptor cmd = serviceProvider.GetRequiredService<CommandDescriptor>();

            Assert.AreEqual("id1", cmd.Id);
            Assert.IsTrue(cmd.IsGroup);
            Assert.IsFalse(cmd.IsRoot);
            Assert.IsFalse(cmd.IsProtected);
        }

        [TestMethod]
        public void AddCommandDescriptorWithRootAndNoGroupShouldError()
        {
            TestHelper.AssertThrowsErrorException(() => terminalBuilder.DefineCommand<MockCommandChecker, MockCommandRunner>("id1", "name1", "prefix1", "desc", isRoot: true, isGroup: false).Add(), TerminalErrors.InvalidConfiguration, "The root command must also be a grouped command. command_id=id1 command_name=name1");
        }

        [TestMethod]
        public void AddCommandDescriptorWithSpecialAnnotationsShouldNotError()
        {
            terminalBuilder.DefineCommand<MockCommandChecker, MockCommandRunner>("id1", "name1", "prefix1", "desc", isRoot: true, isGroup: true, isProtected: true).Add();

            IServiceProvider serviceProvider = terminalBuilder.Services.BuildServiceProvider();
            CommandDescriptor cmd = serviceProvider.GetRequiredService<CommandDescriptor>();

            Assert.IsTrue(cmd.IsGroup);
            Assert.IsTrue(cmd.IsRoot);
            Assert.IsTrue(cmd.IsProtected);
        }

        [TestMethod]
        public void AddCommandShouldCorrectlyInitialize()
        {
            ICommandBuilder commandBuilder = terminalBuilder.DefineCommand<MockCommandChecker, MockCommandRunner>("id1", "name1", "prefix1", "description1");

            // AddCommand does not add ICommandBuilder to service collection.
            var servicesCmdBuilder = terminalBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(ICommandBuilder)));
            Assert.IsNull(servicesCmdBuilder);

            // Command builder creates a new local service collection
            Assert.AreNotEqual(terminalBuilder.Services, commandBuilder.Services);

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
            ICommandBuilder commandBuilder = terminalBuilder.DefineCommand<MockCommandChecker, MockCommandRunner>("id1", "name1", "prefix1", "description1", isRoot: true, isGroup: true, isProtected: true);

            // AddCommand does not add ICommandBuilder to service collection.
            var servicesCmdBuilder = terminalBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(ICommandBuilder)));
            Assert.IsNull(servicesCmdBuilder);

            // Command builder creates a new local service collection
            Assert.AreNotEqual(terminalBuilder.Services, commandBuilder.Services);

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
            terminalBuilder.AddExtractor<MockCommandExtractor, MockArgumentExtractor>();

            var cmd = terminalBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(ICommandExtractor)));
            Assert.IsNotNull(cmd);
            Assert.AreEqual(ServiceLifetime.Transient, cmd.Lifetime);
            Assert.AreEqual(typeof(MockCommandExtractor), cmd.ImplementationType);

            var arg = terminalBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(IOptionExtractor)));
            Assert.IsNotNull(arg);
            Assert.AreEqual(ServiceLifetime.Transient, arg.Lifetime);
            Assert.AreEqual(typeof(MockArgumentExtractor), arg.ImplementationType);
        }

        [TestMethod]
        public void AddPublisherShouldCorrectlyInitialize()
        {
            terminalBuilder.AddExceptionHandler<MockExceptionPublisher>();

            var exe = terminalBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(IExceptionHandler)));
            Assert.IsNotNull(exe);
            Assert.AreEqual(ServiceLifetime.Transient, exe.Lifetime);
            Assert.AreEqual(typeof(MockExceptionPublisher), exe.ImplementationType);
        }

        [TestMethod]
        public void AddRouterShouldCorrectlyInitialize()
        {
            terminalBuilder.AddRouter<MockCommandRouter, MockCommandHandler>();

            var router = terminalBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(ICommandRouter)));
            Assert.IsNotNull(router);
            Assert.AreEqual(ServiceLifetime.Transient, router.Lifetime);
            Assert.AreEqual(typeof(MockCommandRouter), router.ImplementationType);

            var handler = terminalBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(ICommandHandler)));
            Assert.IsNotNull(handler);
            Assert.AreEqual(ServiceLifetime.Transient, handler.Lifetime);
            Assert.AreEqual(typeof(MockCommandHandler), handler.ImplementationType);
        }

        [TestMethod]
        public void AddRoutingShouldCorrectlyInitialize()
        {
            terminalBuilder.AddTerminalRouting<MockRouting, MockRoutingContext, MockRoutingResult>();

            var router = terminalBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(MockRouting)));
            Assert.IsNotNull(router);
            Assert.AreEqual(ServiceLifetime.Singleton, router.Lifetime);
            Assert.AreEqual(typeof(MockRouting), router.ImplementationType);
        }

        [TestMethod]
        public void AddTextHandlerShouldCorrectlyInitialize()
        {
            terminalBuilder.AddTextHandler<UnicodeTextHandler>();

            var comparer = terminalBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(ITextHandler)));
            Assert.IsNotNull(comparer);
            Assert.AreEqual(ServiceLifetime.Transient, comparer.Lifetime);

            // This registers a factory so we build to check the instance
            var serviceProvider = terminalBuilder.Services.BuildServiceProvider();
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
            terminalBuilder = serviceDescriptors.AddTerminalBuilder();
        }

        private ITerminalBuilder terminalBuilder = null!;
    }
}