﻿/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OneImlx.Terminal.Commands;
using OneImlx.Terminal.Commands.Checkers;
using OneImlx.Terminal.Commands.Handlers;
using OneImlx.Terminal.Commands.Mappers;
using OneImlx.Terminal.Commands.Parsers;
using OneImlx.Terminal.Commands.Providers;
using OneImlx.Terminal.Commands.Routers;
using OneImlx.Terminal.Configuration.Options;
using OneImlx.Terminal.Events;
using OneImlx.Terminal.Hosting;
using OneImlx.Terminal.Licensing;
using OneImlx.Terminal.Mocks;
using OneImlx.Terminal.Runtime;
using OneImlx.Terminal.Stores;
using System;
using System.Linq;
using System.Threading;

namespace OneImlx.Terminal.Extensions
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

            var arg = terminalBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(ITerminalEventHandler)));
            Assert.IsNotNull(arg);
            Assert.AreEqual(ServiceLifetime.Singleton, arg.Lifetime);
            Assert.AreEqual(typeof(MockAsyncEventHandler), arg.ImplementationType);
        }

        [TestMethod]
        public void AddOptionCheckerShouldCorrectlyInitialize()
        {
            terminalBuilder.AddOptionChecker<MockOptionMapper, MockOptionChecker>();

            var arg = terminalBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(IOptionChecker)));
            Assert.IsNotNull(arg);
            Assert.AreEqual(ServiceLifetime.Transient, arg.Lifetime);
            Assert.AreEqual(typeof(MockOptionChecker), arg.ImplementationType);

            var map = terminalBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(IDataTypeMapper<Option>)));
            Assert.IsNotNull(map);
            Assert.AreEqual(ServiceLifetime.Transient, map.Lifetime);
            Assert.AreEqual(typeof(MockOptionMapper), map.ImplementationType);
        }

        [TestMethod]
        public void AddArgumentCheckerShouldCorrectlyInitialize()
        {
            terminalBuilder.AddArgumentChecker<MockArgumentMapper, MockArgumentChecker>();

            var arg = terminalBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(IArgumentChecker)));
            Assert.IsNotNull(arg);
            Assert.AreEqual(ServiceLifetime.Transient, arg.Lifetime);
            Assert.AreEqual(typeof(MockArgumentChecker), arg.ImplementationType);

            var map = terminalBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(IDataTypeMapper<Argument>)));
            Assert.IsNotNull(map);
            Assert.AreEqual(ServiceLifetime.Transient, map.Lifetime);
            Assert.AreEqual(typeof(MockArgumentMapper), map.ImplementationType);
        }

        [TestMethod]
        public void AddTerminalOptionsShouldCorrectlyInitialize()
        {
            terminalBuilder.AddConfigurationOptions();

            var serviceDescriptor = terminalBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(TerminalOptions)));
            Assert.IsNotNull(serviceDescriptor);
            Assert.AreEqual(ServiceLifetime.Singleton, serviceDescriptor.Lifetime);
        }

        [TestMethod]
        public void AddStartContextShouldCorrectlyInitialize()
        {
            terminalBuilder.AddStartContext(new TerminalStartContext(TerminalStartMode.Custom, CancellationToken.None, CancellationToken.None));

            var serviceDescriptor = terminalBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(TerminalStartContext)));
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
        public void AddCommandImmutableStoreShouldCorrectlyInitialize()
        {
            terminalBuilder.AddCommandStore<MockImmutableCommandStore>();

            // Check if MockImmutableCommandStore is registered correctly
            var concreteServiceDescriptor = terminalBuilder.Services
                .FirstOrDefault(e => e.ServiceType.Equals(typeof(MockImmutableCommandStore)));
            Assert.IsNotNull(concreteServiceDescriptor);
            Assert.AreEqual(ServiceLifetime.Singleton, concreteServiceDescriptor.Lifetime);

            // Check if IImmutableCommandStore is mapped to MockImmutableCommandStore
            var immutableInterfaceDescriptor = terminalBuilder.Services
                .FirstOrDefault(e => e.ServiceType.Equals(typeof(IImmutableCommandStore)));
            Assert.IsNotNull(immutableInterfaceDescriptor);
            Assert.AreEqual(ServiceLifetime.Singleton, immutableInterfaceDescriptor.Lifetime);

            // Verify the factory for IImmutableCommandStore resolves to MockImmutableCommandStore
            var serviceProvider = terminalBuilder.Services.BuildServiceProvider();
            var immutableCommandStore = serviceProvider.GetService<IImmutableCommandStore>();
            var immutableCommandStore2 = serviceProvider.GetService<IImmutableCommandStore>();
            Assert.IsNotNull(immutableCommandStore);
            Assert.IsInstanceOfType(immutableCommandStore, typeof(MockImmutableCommandStore));
            immutableCommandStore.Should().BeSameAs(immutableCommandStore2);

            // Verify that IMutableCommandStore is not registered
            var mutableCommandStore = serviceProvider.GetService<IMutableCommandStore>();
            Assert.IsNull(mutableCommandStore);
        }

        [TestMethod]
        public void AddCommandMutableStoreShouldCorrectlyInitialize()
        {
            terminalBuilder.AddCommandStore<MockMutableCommandStore>();

            // Check if MockMutableCommandStore is registered correctly
            var concreteServiceDescriptor = terminalBuilder.Services
                .FirstOrDefault(e => e.ServiceType.Equals(typeof(MockMutableCommandStore)));
            Assert.IsNotNull(concreteServiceDescriptor);
            Assert.AreEqual(ServiceLifetime.Singleton, concreteServiceDescriptor.Lifetime);

            // Check if IMutableCommandStore is mapped to MockMutableCommandStore
            var mutableInterfaceDescriptor = terminalBuilder.Services
                .FirstOrDefault(e => e.ServiceType.Equals(typeof(IMutableCommandStore)));
            Assert.IsNotNull(mutableInterfaceDescriptor);
            Assert.AreEqual(ServiceLifetime.Singleton, mutableInterfaceDescriptor.Lifetime);

            // Check if IImmutableCommandStore is mapped to MockMutableCommandStore
            var immutableInterfaceDescriptor = terminalBuilder.Services
                .FirstOrDefault(e => e.ServiceType.Equals(typeof(IImmutableCommandStore)));
            Assert.IsNotNull(immutableInterfaceDescriptor);
            Assert.AreEqual(ServiceLifetime.Singleton, immutableInterfaceDescriptor.Lifetime);

            // Verify the factories for both interfaces resolve to MockMutableCommandStore
            var serviceProvider = terminalBuilder.Services.BuildServiceProvider();
            var mutableCommandStore = serviceProvider.GetService<IMutableCommandStore>();
            var immutableCommandStore = serviceProvider.GetService<IImmutableCommandStore>();
            var mutableCommandStore2 = serviceProvider.GetService<IMutableCommandStore>();
            var immutableCommandStore2 = serviceProvider.GetService<IImmutableCommandStore>();
            mutableCommandStore.Should().BeSameAs(immutableCommandStore);
            mutableCommandStore2.Should().BeSameAs(immutableCommandStore2);
            mutableCommandStore.Should().BeSameAs(mutableCommandStore2);

            Assert.IsNotNull(mutableCommandStore);
            Assert.IsInstanceOfType(mutableCommandStore, typeof(MockMutableCommandStore));

            Assert.IsNotNull(immutableCommandStore);
            Assert.IsInstanceOfType(immutableCommandStore, typeof(MockMutableCommandStore));
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
        public void AddCommandParserShouldCorrectlyInitialize()
        {
            terminalBuilder.AddCommandParser<MockCommandParser, MockCommandRouteParser>();

            var cmd = terminalBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(ICommandParser)));
            Assert.IsNotNull(cmd);
            Assert.AreEqual(ServiceLifetime.Transient, cmd.Lifetime);
            Assert.AreEqual(typeof(MockCommandParser), cmd.ImplementationType);

            var arg = terminalBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(ICommandRouteParser)));
            Assert.IsNotNull(arg);
            Assert.AreEqual(ServiceLifetime.Transient, arg.Lifetime);
            Assert.AreEqual(typeof(MockCommandRouteParser), arg.ImplementationType);
        }

        [TestMethod]
        public void AddLicensingShouldCorrectlyInitialize()
        {
            terminalBuilder.AddLicensing();

            var debugger = terminalBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(ILicenseDebugger)));
            Assert.IsNotNull(debugger);
            Assert.AreEqual(ServiceLifetime.Singleton, debugger.Lifetime);
            Assert.AreEqual(typeof(LicenseDebugger), debugger.ImplementationType);

            var extractor = terminalBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(ILicenseExtractor)));
            Assert.IsNotNull(extractor);
            Assert.AreEqual(ServiceLifetime.Singleton, extractor.Lifetime);
            Assert.AreEqual(typeof(LicenseExtractor), extractor.ImplementationType);

            var checker = terminalBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(ILicenseChecker)));
            Assert.IsNotNull(checker);
            Assert.AreEqual(ServiceLifetime.Singleton, checker.Lifetime);
            Assert.AreEqual(typeof(LicenseChecker), checker.ImplementationType);
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
        public void AddTerminalConsoleShouldCorrectlyInitialize()
        {
            terminalBuilder.AddConsole<TerminalSystemConsole>();

            var exe = terminalBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(ITerminalConsole)));
            Assert.IsNotNull(exe);
            Assert.AreEqual(ServiceLifetime.Singleton, exe.Lifetime);
            Assert.AreEqual(typeof(TerminalSystemConsole), exe.ImplementationType);
        }

        [TestMethod]
        public void AddRouterShouldCorrectlyInitialize()
        {
            terminalBuilder.AddCommandRouter<MockCommandRouter, MockCommandHandler>();

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
        public void AddTerminalRouterShouldCorrectlyInitialize()
        {
            terminalBuilder.AddTerminalRouter<MockTerminalRouter, MockRoutingContext>();

            var router = terminalBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(ITerminalRouter<MockRoutingContext>)));
            Assert.IsNotNull(router);
            Assert.AreEqual(ServiceLifetime.Singleton, router.Lifetime);
            Assert.AreEqual(typeof(MockTerminalRouter), router.ImplementationType);
        }

        [TestMethod]
        public void AddTextHandlerShouldCorrectlyInitialize()
        {
            // AddTextHandler is called within CreateTerminalBuilder
            var comparer = terminalBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(ITextHandler)));
            Assert.IsNotNull(comparer);
            Assert.AreEqual(ServiceLifetime.Singleton, comparer.Lifetime);
            Assert.IsNotNull(comparer.ImplementationInstance);
            Assert.IsNull(comparer.ImplementationType);
            Assert.AreEqual(typeof(UnicodeTextHandler), comparer.ImplementationInstance.GetType());
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

            terminalBuilder = serviceDescriptors.CreateTerminalBuilder(new UnicodeTextHandler());
        }
    }
}