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
using System;
using System.Collections.Generic;
using Xunit;

namespace PerpetualIntelligence.Terminal.Extensions
{
    public class IServiceCollectionExtensionsTests
    {
        [Fact]
        public void CreateTerminalBuilderShouldNotAddAnyServices()
        {
            IServiceCollection? serviceDescriptors = null;

            using var host = Host.CreateDefaultBuilder(Array.Empty<string>()).ConfigureServices(arg =>
            {
                serviceDescriptors = arg;
            }).Build();

            serviceDescriptors.Should().NotBeNull();
            ITerminalBuilder? terminalBuilder = serviceDescriptors!.CreateTerminalBuilder();
            terminalBuilder.Should().NotBeNull()
                           .And.BeOfType<TerminalBuilder>()
                           .And.Match<TerminalBuilder>(tb => ReferenceEquals(serviceDescriptors, tb.Services));

            setupActionCalled.Should().BeFalse();

            AssertCoreServicesNotAdded(host);
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
                arg.AddTerminal(configuration);
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

            AssertCoreServicesNotAdded(host);
        }

        [Fact]
        public void AddTerminalNoConfigShouldInitializeCorrectly()
        {
            using var host = Host.CreateDefaultBuilder(Array.Empty<string>()).ConfigureServices(arg =>
            {
                arg.AddTerminal();
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

            AssertCoreServicesNotAdded(host);
        }

        [Fact]
        public void AddTerminalOptionsShouldInitializeCorrectly()
        {
            using var host = Host.CreateDefaultBuilder(Array.Empty<string>()).ConfigureServices(arg =>
            {
                arg.AddTerminal(SetupAction);
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

            AssertCoreServicesNotAdded(host);
        }

        private void AssertCoreServicesNotAdded(IHost host)
        {
            // Check ICommandRouter is added as a transient
            ICommandRouter? commandRouter = host.Services.GetService<ICommandRouter>();
            commandRouter.Should().BeNull();

            // Check ICommandRouter is added as a transient
            ICommandHandler? commandHandler = host.Services.GetService<ICommandHandler>();
            commandHandler.Should().BeNull();

            // License checker is added as a singleton
            ILicenseChecker? licenseChecker = host.Services.GetService<ILicenseChecker>();
            licenseChecker.Should().BeNull();

            // License extractor is added as a singleton
            ILicenseExtractor? licenseExtractor = host.Services.GetService<ILicenseExtractor>();
            licenseExtractor.Should().BeNull();
        }

        private void SetupAction(TerminalOptions obj)
        {
            setupActionCalled = true;
        }

        private bool setupActionCalled;
    }
}