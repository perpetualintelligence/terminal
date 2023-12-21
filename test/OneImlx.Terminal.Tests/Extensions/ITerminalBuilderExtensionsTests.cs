/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
using Xunit;

namespace OneImlx.Terminal.Extensions
{
    public class ITerminalBuilderExtensionsTests
    {
        private readonly ITerminalBuilder terminalBuilder;

        [Fact]
        public void AddHelpProviderShouldCorrectlyInitialize()
        {
            terminalBuilder.AddHelpProvider<MockHelpProvider>();

            var arg = terminalBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(IHelpProvider)));
            arg.Should().NotBeNull();
            arg.Lifetime.Should().Be(ServiceLifetime.Singleton);
            arg.ImplementationType.Should().Be(typeof(MockHelpProvider));
        }

        [Fact]
        public void AddEventHandlerShouldCorrectlyInitialize()
        {
            terminalBuilder.AddEventHandler<MockAsyncEventHandler>();

            var arg = terminalBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(ITerminalEventHandler)));
            arg.Should().NotBeNull();
            arg.Lifetime.Should().Be(ServiceLifetime.Singleton);
            arg.ImplementationType.Should().Be(typeof(MockAsyncEventHandler));
        }

        [Fact]
        public void AddOptionCheckerShouldCorrectlyInitialize()
        {
            terminalBuilder.AddOptionChecker<MockOptionMapper, MockOptionChecker>();

            var arg = terminalBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(IOptionChecker)));
            arg.Should().NotBeNull();
            arg.Lifetime.Should().Be(ServiceLifetime.Transient);
            arg.ImplementationType.Should().Be(typeof(MockOptionChecker));

            var map = terminalBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(IDataTypeMapper<Option>)));
            map.Should().NotBeNull();
            map.Lifetime.Should().Be(ServiceLifetime.Transient);
            map.ImplementationType.Should().Be(typeof(MockOptionMapper));
        }

        [Fact]
        public void AddArgumentCheckerShouldCorrectlyInitialize()
        {
            terminalBuilder.AddArgumentChecker<MockArgumentMapper, MockArgumentChecker>();

            var arg = terminalBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(IArgumentChecker)));
            arg.Should().NotBeNull();
            arg.Lifetime.Should().Be(ServiceLifetime.Transient);
            arg.ImplementationType.Should().Be(typeof(MockArgumentChecker));

            var map = terminalBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(IDataTypeMapper<Argument>)));
            map.Should().NotBeNull();
            map.Lifetime.Should().Be(ServiceLifetime.Transient);
            map.ImplementationType.Should().Be(typeof(MockArgumentMapper));
        }

        [Fact]
        public void AddTerminalOptionsShouldCorrectlyInitialize()
        {
            terminalBuilder.AddConfigurationOptions();

            var serviceDescriptor = terminalBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(TerminalOptions)));
            serviceDescriptor.Should().NotBeNull();
            serviceDescriptor!.Lifetime.Should().Be(ServiceLifetime.Singleton);
        }

        [Fact]
        public void AddStartContextShouldCorrectlyInitialize()
        {
            terminalBuilder.AddStartContext(new TerminalStartContext(TerminalStartMode.Custom, CancellationToken.None, CancellationToken.None));

            var serviceDescriptor = terminalBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(TerminalStartContext)));
            serviceDescriptor.Should().NotBeNull();
            serviceDescriptor!.Lifetime.Should().Be(ServiceLifetime.Singleton);
        }

        [Fact]
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

        [Fact]
        public void AddCommandDescriptorShouldCorrectlyInitializeCheckerAndRunner()
        {
            terminalBuilder.DefineCommand<MockCommandChecker, MockCommandRunner>("id1", "name1", "desc", CommandType.SubCommand, CommandFlags.None).Add();

            ServiceDescriptor? cmdDescriptor = terminalBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(CommandDescriptor)));
            cmdDescriptor.Should().NotBeNull();
            cmdDescriptor!.Lifetime.Should().Be(ServiceLifetime.Singleton);
            CommandDescriptor? impInstance = (CommandDescriptor?)cmdDescriptor.ImplementationInstance;
            impInstance.Should().NotBeNull();
            impInstance!.Id.Should().Be("id1");
            impInstance.Name.Should().Be("name1");
            impInstance.Runner.Should().Be(typeof(MockCommandRunner));
            impInstance.Checker.Should().Be(typeof(MockCommandChecker));
            impInstance.Type.Should().Be(CommandType.SubCommand);

            var cmdRunner = terminalBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(MockCommandRunner)));
            cmdRunner.Should().NotBeNull();
            cmdRunner!.Lifetime.Should().Be(ServiceLifetime.Transient);

            var cmdChecker = terminalBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(MockCommandChecker)));
            cmdChecker.Should().NotBeNull();
            cmdChecker!.Lifetime.Should().Be(ServiceLifetime.Transient);
        }

        [Fact]
        public void AddCommandImmutableStoreShouldCorrectlyInitialize()
        {
            terminalBuilder.AddCommandStore<MockImmutableCommandStore>();

            // Check if MockImmutableCommandStore is registered correctly
            var concreteServiceDescriptor = terminalBuilder.Services
                .FirstOrDefault(e => e.ServiceType.Equals(typeof(MockImmutableCommandStore)));
            concreteServiceDescriptor.Should().NotBeNull();
            concreteServiceDescriptor!.Lifetime.Should().Be(ServiceLifetime.Singleton);

            // Check if IImmutableCommandStore is mapped to MockImmutableCommandStore
            var immutableInterfaceDescriptor = terminalBuilder.Services
                .FirstOrDefault(e => e.ServiceType.Equals(typeof(IImmutableCommandStore)));
            immutableInterfaceDescriptor.Should().NotBeNull();
            immutableInterfaceDescriptor!.Lifetime.Should().Be(ServiceLifetime.Singleton);

            // Verify the factory for IImmutableCommandStore resolves to MockImmutableCommandStore
            var serviceProvider = terminalBuilder.Services.BuildServiceProvider();
            var immutableCommandStore = serviceProvider.GetService<IImmutableCommandStore>();
            var immutableCommandStore2 = serviceProvider.GetService<IImmutableCommandStore>();
            immutableCommandStore.Should().NotBeNull();
            immutableCommandStore.Should().BeOfType<MockImmutableCommandStore>();
            immutableCommandStore.Should().BeSameAs(immutableCommandStore2);

            // Verify that IMutableCommandStore is not registered
            var mutableCommandStore = serviceProvider.GetService<IMutableCommandStore>();
            mutableCommandStore.Should().BeNull();
        }

        [Fact]
        public void AddCommandMutableStoreShouldCorrectlyInitialize()
        {
            terminalBuilder.AddCommandStore<MockMutableCommandStore>();

            // Check if MockMutableCommandStore is registered correctly
            var concreteServiceDescriptor = terminalBuilder.Services
                .FirstOrDefault(e => e.ServiceType.Equals(typeof(MockMutableCommandStore)));
            concreteServiceDescriptor.Should().NotBeNull();
            concreteServiceDescriptor!.Lifetime.Should().Be(ServiceLifetime.Singleton);

            // Check if IMutableCommandStore is mapped to MockMutableCommandStore
            var mutableInterfaceDescriptor = terminalBuilder.Services
                .FirstOrDefault(e => e.ServiceType.Equals(typeof(IMutableCommandStore)));
            mutableInterfaceDescriptor.Should().NotBeNull();
            mutableInterfaceDescriptor!.Lifetime.Should().Be(ServiceLifetime.Singleton);

            // Check if IImmutableCommandStore is mapped to MockMutableCommandStore
            var immutableInterfaceDescriptor = terminalBuilder.Services
                .FirstOrDefault(e => e.ServiceType.Equals(typeof(IImmutableCommandStore)));
            immutableInterfaceDescriptor.Should().NotBeNull();
            immutableInterfaceDescriptor!.Lifetime.Should().Be(ServiceLifetime.Singleton);

            // Verify the factories for both interfaces resolve to MockMutableCommandStore
            var serviceProvider = terminalBuilder.Services.BuildServiceProvider();
            var mutableCommandStore = serviceProvider.GetService<IMutableCommandStore>();
            var immutableCommandStore = serviceProvider.GetService<IImmutableCommandStore>();
            var mutableCommandStore2 = serviceProvider.GetService<IMutableCommandStore>();
            var immutableCommandStore2 = serviceProvider.GetService<IImmutableCommandStore>();
            mutableCommandStore.Should().BeSameAs(immutableCommandStore);
            mutableCommandStore2.Should().BeSameAs(immutableCommandStore2);
            mutableCommandStore.Should().BeSameAs(mutableCommandStore2);

            mutableCommandStore.Should().NotBeNull();
            mutableCommandStore.Should().BeOfType<MockMutableCommandStore>();

            immutableCommandStore.Should().NotBeNull();
            immutableCommandStore.Should().BeOfType<MockMutableCommandStore>();
        }

        [Fact]
        public void AddCommandDescriptorWithGroupAndNoRootShouldNotError()
        {
            terminalBuilder.DefineCommand<MockCommandChecker, MockCommandRunner>("id1", "name1", "desc", CommandType.Group, CommandFlags.None).Add();

            IServiceProvider serviceProvider = terminalBuilder.Services.BuildServiceProvider();
            CommandDescriptor cmd = serviceProvider.GetRequiredService<CommandDescriptor>();

            cmd.Id.Should().Be("id1");
            CommandType.Group.Should().Be(CommandType.Group);
        }

        [Fact]
        public void AddCommandDescriptorWithSpecialAnnotationsShouldNotError()
        {
            terminalBuilder.DefineCommand<MockCommandChecker, MockCommandRunner>("id1", "name1", "desc", CommandType.Root, CommandFlags.Protected).Add();

            IServiceProvider serviceProvider = terminalBuilder.Services.BuildServiceProvider();
            CommandDescriptor cmd = serviceProvider.GetRequiredService<CommandDescriptor>();

            cmd.Type.Should().Be(CommandType.Root);
            cmd.Flags.Should().Be(CommandFlags.Protected);
        }

        [Fact]
        public void AddCommandDescriptorWithSpecialAnnotationsFlagsShouldNotError()
        {
            terminalBuilder.DefineCommand<MockCommandChecker, MockCommandRunner>("id1", "name1", "desc", CommandType.Group, CommandFlags.Protected | CommandFlags.Obsolete).Add();

            IServiceProvider serviceProvider = terminalBuilder.Services.BuildServiceProvider();
            CommandDescriptor cmd = serviceProvider.GetRequiredService<CommandDescriptor>();

            cmd.Type.Should().Be(CommandType.Group);
            cmd.Flags.Should().Be(CommandFlags.Protected | CommandFlags.Obsolete);
        }

        [Fact]
        public void AddCommandShouldCorrectlyInitialize()
        {
            ICommandBuilder commandBuilder = terminalBuilder.DefineCommand<MockCommandChecker, MockCommandRunner>("id1", "name1", "description1", CommandType.SubCommand, CommandFlags.None);

            // AddCommand does not add ICommandBuilder to service collection.
            var servicesCmdBuilder = terminalBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(ICommandBuilder)));
            servicesCmdBuilder.Should().BeNull();

            // Command builder creates a new local service collection
            terminalBuilder.Services.Should().NotBeSameAs(commandBuilder.Services);

            // Lifetime of command builder service
            var serviceDescriptor = commandBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(CommandDescriptor)));
            serviceDescriptor.Should().NotBeNull();
            serviceDescriptor!.Lifetime.Should().Be(ServiceLifetime.Singleton);

            // Check the instance
            var serviceProvider = commandBuilder.Services.BuildServiceProvider();
            var instance = serviceProvider.GetRequiredService<CommandDescriptor>();
            instance.Id.Should().Be("id1");
            instance.Name.Should().Be("name1");
            instance.Description.Should().Be("description1");
            instance.Checker.Should().Be(typeof(MockCommandChecker));
            instance.Runner.Should().Be(typeof(MockCommandRunner));
            instance.Type.Should().Be(CommandType.SubCommand);
            instance.Flags.Should().Be(CommandFlags.None);
        }

        [Fact]
        public void AddCommandSpecialAnnotationsShouldCorrectlyInitialize()
        {
            ICommandBuilder commandBuilder = terminalBuilder.DefineCommand<MockCommandChecker, MockCommandRunner>("id1", "name1", "description1", CommandType.Root, CommandFlags.Protected);

            // AddCommand does not add ICommandBuilder to service collection.
            var servicesCmdBuilder = terminalBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(ICommandBuilder)));
            servicesCmdBuilder.Should().BeNull();

            // Command builder creates a new local service collection
            terminalBuilder.Services.Should().NotBeSameAs(commandBuilder.Services);

            // Lifetime of command builder service
            var serviceDescriptor = commandBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(CommandDescriptor)));
            serviceDescriptor.Should().NotBeNull();
            serviceDescriptor!.Lifetime.Should().Be(ServiceLifetime.Singleton);

            // Check the instance
            var serviceProvider = commandBuilder.Services.BuildServiceProvider();
            var instance = serviceProvider.GetRequiredService<CommandDescriptor>();
            instance.Id.Should().Be("id1");
            instance.Name.Should().Be("name1");
            instance.Description.Should().Be("description1");
            instance.Checker.Should().Be(typeof(MockCommandChecker));
            instance.Runner.Should().Be(typeof(MockCommandRunner));
            instance.Type.Should().Be(CommandType.Root);
            instance.Flags.Should().Be(CommandFlags.Protected);
        }

        [Fact]
        public void AddCommandParserShouldCorrectlyInitialize()
        {
            terminalBuilder.AddCommandParser<MockCommandParser, MockCommandRouteParser>();

            var cmd = terminalBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(ICommandParser)));
            cmd.Should().NotBeNull();
            cmd.Lifetime.Should().Be(ServiceLifetime.Transient);
            cmd.ImplementationType.Should().Be(typeof(MockCommandParser));

            var arg = terminalBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(ICommandRouteParser)));
            arg.Should().NotBeNull();
            arg.Lifetime.Should().Be(ServiceLifetime.Transient);
            arg.ImplementationType.Should().Be(typeof(MockCommandRouteParser));
        }

        [Fact]
        public void AddLicensingShouldCorrectlyInitialize()
        {
            terminalBuilder.AddLicensing();

            var debugger = terminalBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(ILicenseDebugger)));
            debugger.Should().NotBeNull();
            debugger!.Lifetime.Should().Be(ServiceLifetime.Singleton);
            debugger.ImplementationType.Should().Be(typeof(LicenseDebugger));

            var extractor = terminalBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(ILicenseExtractor)));
            extractor.Should().NotBeNull();
            extractor!.Lifetime.Should().Be(ServiceLifetime.Singleton);
            extractor.ImplementationType.Should().Be(typeof(LicenseExtractor));

            var checker = terminalBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(ILicenseChecker)));
            checker.Should().NotBeNull();
            checker!.Lifetime.Should().Be(ServiceLifetime.Singleton);
            checker.ImplementationType.Should().Be(typeof(LicenseChecker));
        }

        [Fact]
        public void AddPublisherShouldCorrectlyInitialize()
        {
            terminalBuilder.AddExceptionHandler<MockExceptionPublisher>();

            var exe = terminalBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(IExceptionHandler)));
            exe.Should().NotBeNull();
            exe.Lifetime.Should().Be(ServiceLifetime.Transient);
            exe.ImplementationType.Should().Be(typeof(MockExceptionPublisher));
        }

        [Fact]
        public void AddTerminalConsoleShouldCorrectlyInitialize()
        {
            terminalBuilder.AddConsole<TerminalSystemConsole>();

            var exe = terminalBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(ITerminalConsole)));
            exe.Should().NotBeNull();
            exe.Lifetime.Should().Be(ServiceLifetime.Singleton);
            exe.ImplementationType.Should().Be(typeof(TerminalSystemConsole));
        }

        [Fact]
        public void AddRouterShouldCorrectlyInitialize()
        {
            terminalBuilder.AddCommandRouter<MockCommandRouter, MockCommandHandler>();

            var router = terminalBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(ICommandRouter)));
            router.Should().NotBeNull();
            router!.Lifetime.Should().Be(ServiceLifetime.Transient);
            router.ImplementationType.Should().Be(typeof(MockCommandRouter));

            var handler = terminalBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(ICommandHandler)));
            handler.Should().NotBeNull();
            handler!.Lifetime.Should().Be(ServiceLifetime.Transient);
            handler!.ImplementationType.Should().Be(typeof(MockCommandHandler));
        }

        [Fact]
        public void AddTerminalRouterShouldCorrectlyInitialize()
        {
            terminalBuilder.AddTerminalRouter<MockTerminalRouter, MockRoutingContext>();

            var router = terminalBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(ITerminalRouter<MockRoutingContext>)));
            router.Should().NotBeNull();
            router!.Lifetime.Should().Be(ServiceLifetime.Singleton);
            router!.ImplementationType.Should().Be(typeof(MockTerminalRouter));
        }

        [Fact]
        public void AddTextHandlerShouldCorrectlyInitialize()
        {
            // AddTextHandler is called within CreateTerminalBuilder
            var comparer = terminalBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(ITextHandler)));
            comparer.Should().NotBeNull();
            comparer!.Lifetime.Should().Be(ServiceLifetime.Singleton);
            comparer!.ImplementationInstance.Should().NotBeNull();
            comparer.ImplementationType.Should().BeNull();
            comparer.ImplementationInstance!.GetType().Should().Be(typeof(UnicodeTextHandler));
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