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
using System;
using System.Linq;

namespace PerpetualIntelligence.Terminal.Extensions
{
    [TestClass]
    public class ITerminalBuilderExtensionsTests
    {
        private readonly ITerminalBuilder terminalBuilder;

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
            terminalBuilder.DefineCommand<MockCommandChecker, MockCommandRunner>("id1", "name1", "desc", CommandType.SubCommand, CommandFlags.None).Add();
            terminalBuilder.DefineCommand<MockCommandChecker, MockCommandRunner>("id1", "name1", "desc", CommandType.SubCommand, CommandFlags.None).Add();

            var sp = terminalBuilder.Services.BuildServiceProvider();
            var cmds = sp.GetServices<CommandDescriptor>();
            cmds.Count().Should().Be(2);

            cmds.First().Id.Equals("id1");
            cmds.Last().Id.Equals("id1");
        }

        [TestMethod]
        public void AddCommandDescriptorShouldCorrectlyInitializeCheckerAndRunner()
        {
            terminalBuilder.DefineCommand<MockCommandChecker, MockCommandRunner>("id1", "name1", "desc", CommandType.SubCommand, CommandFlags.None).Add();

            var cmdDescriptor = terminalBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(CommandDescriptor)));
            Assert.IsNotNull(cmdDescriptor);
            Assert.AreEqual(ServiceLifetime.Singleton, cmdDescriptor.Lifetime);
            CommandDescriptor? impInstance = (CommandDescriptor?)cmdDescriptor.ImplementationInstance;
            Assert.IsNotNull(impInstance);
            Assert.AreEqual("id1", impInstance.Id);
            Assert.AreEqual("name1", impInstance.Name);
            Assert.AreEqual(typeof(MockCommandRunner), impInstance.Runner);
            Assert.AreEqual(typeof(MockCommandChecker), impInstance.Checker);
            Assert.AreEqual(CommandType.SubCommand, impInstance.Type);

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
            terminalBuilder.DefineCommand<MockCommandChecker, MockCommandRunner>("id1", "name1", "desc", CommandType.Group, CommandFlags.None).Add();

            IServiceProvider serviceProvider = terminalBuilder.Services.BuildServiceProvider();
            CommandDescriptor cmd = serviceProvider.GetRequiredService<CommandDescriptor>();

            Assert.AreEqual("id1", cmd.Id);
            Assert.AreEqual(CommandType.Group, cmd.Type);
        }

        [TestMethod]
        public void AddCommandDescriptorWithSpecialAnnotationsShouldNotError()
        {
            terminalBuilder.DefineCommand<MockCommandChecker, MockCommandRunner>("id1", "name1", "desc", CommandType.Root, CommandFlags.Protected).Add();

            IServiceProvider serviceProvider = terminalBuilder.Services.BuildServiceProvider();
            CommandDescriptor cmd = serviceProvider.GetRequiredService<CommandDescriptor>();

            Assert.AreEqual(CommandType.Root, cmd.Type);
            Assert.AreEqual(CommandFlags.Protected, cmd.Flags);
        }

        [TestMethod]
        public void AddCommandDescriptorWithSpecialAnnotationsFlagsShouldNotError()
        {
            terminalBuilder.DefineCommand<MockCommandChecker, MockCommandRunner>("id1", "name1", "desc", CommandType.Group, CommandFlags.Protected | CommandFlags.Obsolete).Add();

            IServiceProvider serviceProvider = terminalBuilder.Services.BuildServiceProvider();
            CommandDescriptor cmd = serviceProvider.GetRequiredService<CommandDescriptor>();

            Assert.AreEqual(CommandType.Group, cmd.Type);
            Assert.AreEqual(CommandFlags.Protected | CommandFlags.Obsolete, cmd.Flags);
        }

        [TestMethod]
        public void AddCommandShouldCorrectlyInitialize()
        {
            ICommandBuilder commandBuilder = terminalBuilder.DefineCommand<MockCommandChecker, MockCommandRunner>("id1", "name1", "description1", CommandType.SubCommand, CommandFlags.None);

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
            Assert.AreEqual("description1", instance.Description);
            Assert.AreEqual(typeof(MockCommandChecker), instance.Checker);
            Assert.AreEqual(typeof(MockCommandRunner), instance.Runner);
            Assert.AreEqual(CommandType.SubCommand, instance.Type);
            Assert.AreEqual(CommandFlags.None, instance.Flags);
        }

        [TestMethod]
        public void AddCommandSpecialAnnotationsShouldCorrectlyInitialize()
        {
            ICommandBuilder commandBuilder = terminalBuilder.DefineCommand<MockCommandChecker, MockCommandRunner>("id1", "name1", "description1", CommandType.Root, CommandFlags.Protected);

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
            Assert.AreEqual("description1", instance.Description);
            Assert.AreEqual(typeof(MockCommandChecker), instance.Checker);
            Assert.AreEqual(typeof(MockCommandRunner), instance.Runner);
            Assert.AreEqual(CommandType.Root, instance.Type);
            Assert.AreEqual(CommandFlags.Protected, instance.Flags);
        }

        [TestMethod]
        public void AddExtractorShouldCorrectlyInitialize()
        {
            terminalBuilder.AddExtractor<MockCommandExtractor, MockCommandRouteParser>();

            var cmd = terminalBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(ICommandExtractor)));
            Assert.IsNotNull(cmd);
            Assert.AreEqual(ServiceLifetime.Transient, cmd.Lifetime);
            Assert.AreEqual(typeof(MockCommandExtractor), cmd.ImplementationType);

            var arg = terminalBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(ICommandRouteParser)));
            Assert.IsNotNull(arg);
            Assert.AreEqual(ServiceLifetime.Transient, arg.Lifetime);
            Assert.AreEqual(typeof(MockCommandRouteParser), arg.ImplementationType);
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

        public ITerminalBuilderExtensionsTests()
        {
            IServiceCollection? serviceDescriptors = null;
            using var host = Host.CreateDefaultBuilder(Array.Empty<string>()).ConfigureServices(arg =>
            {
                serviceDescriptors = arg;
            }).Build();

            if (serviceDescriptors is null)
            {
                throw new InvalidOperationException("Service descriptors not initialized.");
            }
            terminalBuilder = serviceDescriptors.AddTerminal();

            Assert.IsNotNull(serviceDescriptors);
        }
    }
}