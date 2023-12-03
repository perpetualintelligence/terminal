/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PerpetualIntelligence.Terminal.Commands.Handlers;
using PerpetualIntelligence.Terminal.Commands.Routers;
using PerpetualIntelligence.Terminal.Configuration.Options;
using PerpetualIntelligence.Terminal.Hosting;
using PerpetualIntelligence.Terminal.Licensing;
using PerpetualIntelligence.Terminal.Stores;
using System;
using System.Collections.Generic;
using Xunit;

namespace PerpetualIntelligence.Terminal.Extensions
{
    public class IServiceCollectionExtensionsTests
    {
        [Fact]
        public void CreateTerminalBuilder_Only_Adds_TextHandler()
        {
            IServiceCollection? serviceDescriptors = null;
            ITerminalBuilder? terminalBuilder = null;
            AsciiTextHandler textHandler = new();

            using var host = Host.CreateDefaultBuilder(Array.Empty<string>()).ConfigureServices(arg =>
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
            ITextHandler? fromServices = host.Services.GetService<ITextHandler>();
            fromServices.Should().NotBeNull();
            fromServices.Should().BeSameAs(textHandler);

            AssertNoCoreServices(host);
        }

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

            using var host = Host.CreateDefaultBuilder(Array.Empty<string>()).ConfigureServices(arg =>
            {
                arg.AddTerminal<InMemoryImmutableCommandStore, UnicodeTextHandler>(new UnicodeTextHandler(), configuration);
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
        public void AddTerminalNoConfigShouldInitializeCorrectly()
        {
            using var host = Host.CreateDefaultBuilder(Array.Empty<string>()).ConfigureServices(arg =>
            {
                arg.AddTerminal<InMemoryImmutableCommandStore, UnicodeTextHandler>(new UnicodeTextHandler());
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
            using var host = Host.CreateDefaultBuilder(Array.Empty<string>()).ConfigureServices(arg =>
            {
                arg.AddTerminal<InMemoryImmutableCommandStore, UnicodeTextHandler>(new UnicodeTextHandler(), SetupAction);
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

        private void AssertCoreServices(IHost host)
        {
            ICommandRouter? commandRouter = host.Services.GetService<ICommandRouter>();
            commandRouter.Should().BeNull();

            ICommandHandler? commandHandler = host.Services.GetService<ICommandHandler>();
            commandHandler.Should().BeNull();

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