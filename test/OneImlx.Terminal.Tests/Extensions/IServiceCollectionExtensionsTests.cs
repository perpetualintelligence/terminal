/*
    Copyright 2024 (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OneImlx.Terminal.Commands;
using OneImlx.Terminal.Commands.Checkers;
using OneImlx.Terminal.Commands.Handlers;
using OneImlx.Terminal.Commands.Mappers;
using OneImlx.Terminal.Commands.Parsers;
using OneImlx.Terminal.Commands.Routers;
using OneImlx.Terminal.Configuration.Options;
using OneImlx.Terminal.Hosting;
using OneImlx.Terminal.Licensing;
using OneImlx.Terminal.Runtime;
using OneImlx.Terminal.Stores;
using System;
using System.Collections.Generic;
using Xunit;

namespace OneImlx.Terminal.Extensions
{
    public class IServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddTerminalConfigurationShouldInitializeCorrectly()
        {
            var myConfiguration = new Dictionary<string, string>
            {
                {"Key1", "Value1"},
                {"Nested:Key1", "NestedValue1"},
                {"Nested:Key2", "NestedValue2"}
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration!)
                .Build();

            using var host = Host.CreateDefaultBuilder([]).ConfigureServices(arg =>
            {
                arg.AddTerminal<TerminalInMemoryCommandStore, TerminalUnicodeTextHandler>(new TerminalUnicodeTextHandler(), configuration);
            }).Build();

            // Check Options are added
            TerminalOptions? terminalOptions = host.Services.GetService<TerminalOptions>();
            terminalOptions.Should().NotBeNull();

            // Options is singleton
            TerminalOptions? cliOptions2 = host.Services.GetService<TerminalOptions>();
            TerminalOptions? cliOptions3 = host.Services.GetService<TerminalOptions>();
            cliOptions2.Should().BeSameAs(terminalOptions);
            cliOptions3.Should().BeSameAs(terminalOptions);

            setupActionCalled.Should().BeFalse();

            AssertCoreServices(host);
        }

        [Fact]
        public void AddTerminalConsole_ShouldInitializeCorrectly()
        {
            var services = new ServiceCollection();
            services.AddLogging();

            var textHandler = new TerminalAsciiTextHandler();

            services.AddTerminalConsole<TerminalInMemoryCommandStore, TerminalAsciiTextHandler, TerminalConsoleHelpProvider, TerminalConsoleExceptionHandler, TerminalSystemConsole>(
                textHandler,
                options => { }
                                                                                                                                                                                   );

            var provider = services.BuildServiceProvider();

            // Type services
            provider.GetService<ITerminalConsole>().Should().BeOfType<TerminalSystemConsole>();
            provider.GetService<ITerminalHelpProvider>().Should().BeOfType<TerminalConsoleHelpProvider>();
            provider.GetService<ITerminalCommandStore>().Should().BeOfType<TerminalInMemoryCommandStore>();
            provider.GetService<ITerminalTextHandler>().Should().BeOfType<TerminalAsciiTextHandler>();
            provider.GetService<ITerminalExceptionHandler>().Should().BeOfType<TerminalConsoleExceptionHandler>();

            // Command Router
            provider.GetService<ICommandRouter>().Should().BeOfType<CommandRouter>();
            provider.GetService<ICommandHandler>().Should().BeOfType<CommandHandler>();
            provider.GetService<ICommandRuntime>().Should().BeOfType<CommandRuntime>();

            // Command Parser
            provider.GetService<ICommandParser>().Should().BeOfType<CommandParser>();
            provider.GetService<ICommandRouteParser>().Should().BeOfType<CommandRouteParser>();

            // Option and Argument Checkers
            provider.GetService<IOptionChecker>().Should().BeOfType<OptionChecker>();
            provider.GetService<IDataTypeMapper<Option>>().Should().BeOfType<DataTypeMapper<Option>>();

            // Argument checker
            provider.GetService<IArgumentChecker>().Should().BeOfType<ArgumentChecker>();
            provider.GetService<IDataTypeMapper<Argument>>().Should().BeOfType<DataTypeMapper<Argument>>();

            // Exception Handler
            provider.GetService<ITerminalExceptionHandler>().Should().BeOfType<TerminalConsoleExceptionHandler>();

            // Licensing Services
            provider.GetService<ILicenseChecker>().Should().BeOfType<LicenseChecker>();
            provider.GetService<ILicenseExtractor>().Should().BeOfType<LicenseExtractor>();
            provider.GetService<ILicenseDebugger>().Should().BeOfType<LicenseDebugger>();

            // Terminal router
            //provider.GetService<ITerminalRouter<TerminalConsoleRouterContext>>().Should().BeOfType<TerminalConsoleRouter>();
        }

        [Fact]
        public void AddTerminalDefault_ShouldInitializeCorrectly()
        {
            var services = new ServiceCollection();
            services.AddLogging();

            var textHandler = new TerminalAsciiTextHandler();

            services.AddTerminalDefault<TerminalInMemoryCommandStore, TerminalAsciiTextHandler, TerminalLoggerHelpProvider, TerminalLoggerExceptionHandler>
            (
                textHandler,
                options => { }
            );

            var provider = services.BuildServiceProvider();
            provider.GetService<ITerminalConsole>().Should().BeNull();
            provider.GetService<ITerminalHelpProvider>().Should().BeOfType<TerminalLoggerHelpProvider>();
            provider.GetService<ITerminalCommandStore>().Should().BeOfType<TerminalInMemoryCommandStore>();
            provider.GetService<ITerminalTextHandler>().Should().BeOfType<TerminalAsciiTextHandler>();
            provider.GetService<ITerminalExceptionHandler>().Should().BeOfType<TerminalLoggerExceptionHandler>();

            provider.GetService<ITerminalCommandStore>().Should().NotBeNull();
            provider.GetService<ITerminalTextHandler>().Should().NotBeNull();
            provider.GetService<ITerminalHelpProvider>().Should().NotBeNull();
            provider.GetService<ITerminalExceptionHandler>().Should().NotBeNull();

            provider.GetService<ICommandRouter>().Should().NotBeNull();
            provider.GetService<ICommandHandler>().Should().NotBeNull();
            provider.GetService<ICommandRuntime>().Should().NotBeNull();
            provider.GetService<ILicenseChecker>().Should().NotBeNull();
        }

        [Fact]
        public void AddTerminalNoConfigShouldInitializeCorrectly()
        {
            using var host = Host.CreateDefaultBuilder([]).ConfigureServices(arg =>
            {
                arg.AddTerminal<TerminalInMemoryCommandStore, TerminalUnicodeTextHandler>(new TerminalUnicodeTextHandler());
            }).Build();

            // Check Options are added
            TerminalOptions? terminalOptions = host.Services.GetService<TerminalOptions>();
            terminalOptions.Should().NotBeNull();

            // Options is singleton
            TerminalOptions? cliOptions2 = host.Services.GetService<TerminalOptions>();
            TerminalOptions? cliOptions3 = host.Services.GetService<TerminalOptions>();
            cliOptions2.Should().BeSameAs(terminalOptions);
            cliOptions3.Should().BeSameAs(terminalOptions);

            setupActionCalled.Should().BeFalse();

            AssertCoreServices(host);
        }

        [Fact]
        public void AddTerminalOptionsShouldInitializeCorrectly()
        {
            using var host = Host.CreateDefaultBuilder([]).ConfigureServices(arg =>
            {
                arg.AddTerminal<TerminalInMemoryCommandStore, TerminalUnicodeTextHandler>(new TerminalUnicodeTextHandler(), SetupAction);
            }).Build();

            // Check Options are added
            TerminalOptions? terminalOptions = host.Services.GetService<TerminalOptions>();
            terminalOptions.Should().NotBeNull();

            // Options is singleton
            TerminalOptions? cliOptions2 = host.Services.GetService<TerminalOptions>();
            TerminalOptions? cliOptions3 = host.Services.GetService<TerminalOptions>();
            cliOptions2.Should().BeSameAs(terminalOptions);
            cliOptions3.Should().BeSameAs(terminalOptions);

            setupActionCalled.Should().BeTrue();

            AssertCoreServices(host);
        }

        [Fact]
        public void CreateTerminalBuilder_Only_Adds_TextHandler()
        {
            IServiceCollection? serviceDescriptors = null;
            ITerminalBuilder? terminalBuilder = null;
            TerminalAsciiTextHandler textHandler = new();

            using var host = Host.CreateDefaultBuilder([]).ConfigureServices(arg =>
            {
                serviceDescriptors = arg;
                terminalBuilder = serviceDescriptors!.CreateTerminalBuilder(textHandler);
            }).Build();

            serviceDescriptors.Should().NotBeNull();

            terminalBuilder.Should().NotBeNull()
                           .And.BeOfType<TerminalBuilder>()
                           .And.Match<TerminalBuilder>(tb => ReferenceEquals(serviceDescriptors, tb.Services));

            setupActionCalled.Should().BeFalse();

            // Ensure text handler added
            ITerminalTextHandler? fromServices = host.Services.GetService<ITerminalTextHandler>();
            fromServices.Should().NotBeNull();
            fromServices.Should().BeSameAs(textHandler);

            AssertNoCoreServices(host);
        }

        private void AssertCoreServices(IHost host)
        {
            ICommandRouter? commandRouter = host.Services.GetService<ICommandRouter>();
            commandRouter.Should().BeNull();

            ICommandHandler? commandHandler = host.Services.GetService<ICommandHandler>();
            commandHandler.Should().BeNull();

            ICommandRuntime? commandRuntime = host.Services.GetService<ICommandRuntime>();
            commandRuntime.Should().BeNull();

            ILicenseChecker? licenseChecker = host.Services.GetService<ILicenseChecker>();
            licenseChecker.Should().NotBeNull();

            ILicenseExtractor? licenseExtractor = host.Services.GetService<ILicenseExtractor>();
            licenseExtractor.Should().NotBeNull();

            ILicenseDebugger? LicenseDebugger = host.Services.GetService<ILicenseDebugger>();
            LicenseDebugger.Should().NotBeNull();
        }

        private void AssertNoCoreServices(IHost host)
        {
            ICommandRouter? commandRouter = host.Services.GetService<ICommandRouter>();
            commandRouter.Should().BeNull();

            ICommandHandler? commandHandler = host.Services.GetService<ICommandHandler>();
            commandHandler.Should().BeNull();

            ICommandRuntime? commandRuntime = host.Services.GetService<ICommandRuntime>();
            commandRuntime.Should().BeNull();

            ILicenseChecker? licenseChecker = host.Services.GetService<ILicenseChecker>();
            licenseChecker.Should().BeNull();

            ILicenseExtractor? licenseExtractor = host.Services.GetService<ILicenseExtractor>();
            licenseExtractor.Should().BeNull();

            ILicenseDebugger? LicenseDebugger = host.Services.GetService<ILicenseDebugger>();
            LicenseDebugger.Should().BeNull();
        }

        private void SetupAction(TerminalOptions obj)
        {
            setupActionCalled = true;
        }

        private bool setupActionCalled;
    }
}
