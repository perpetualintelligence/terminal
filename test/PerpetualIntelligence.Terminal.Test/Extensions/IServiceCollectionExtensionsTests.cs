﻿/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerpetualIntelligence.Terminal.Commands.Handlers;
using PerpetualIntelligence.Terminal.Commands.Routers;
using PerpetualIntelligence.Terminal.Configuration.Options;
using PerpetualIntelligence.Terminal.Hosting;
using PerpetualIntelligence.Terminal.Licensing;
using PerpetualIntelligence.Terminal.Mocks;
using PerpetualIntelligence.Test;
using PerpetualIntelligence.Test.Services;
using System;
using System.Collections.Generic;

namespace PerpetualIntelligence.Terminal.Extensions
{
    [TestClass]
    public class IServiceCollectionExtensionsTests : InitializerTests
    {
        public IServiceCollectionExtensionsTests() : base(TestLogger.Create<IServiceCollectionExtensionsTests>())
        {
        }

        [TestMethod]
        public void AddCliBuilderShouldPopulateCorrectly()
        {
            IServiceCollection? serviceDescriptors = null;

            using var host = Host.CreateDefaultBuilder(Array.Empty<string>()).ConfigureServices(arg =>
            {
                serviceDescriptors = arg;
            }).Build();

            Assert.IsNotNull(serviceDescriptors);
            ITerminalBuilder? terminalBuilder = serviceDescriptors.AddCliBuilder();
            Assert.IsNotNull(terminalBuilder);
            Assert.IsInstanceOfType(terminalBuilder, typeof(TerminalBuilder));
            Assert.IsTrue(ReferenceEquals(serviceDescriptors, terminalBuilder.Services));

            Assert.IsFalse(setupActionCalled);
        }

        [TestMethod]
        public void AddCliConfigurationShouldInitializeCorrectly()
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
                arg.AddCli(configuration)
                .AddExtractor<MockCommandExtractor, MockArgumentExtractor>();
            }).Build();

            AssertHostServices(host);

            Assert.IsFalse(setupActionCalled);
        }

        [TestMethod]
        public void AddCliDefaultShouldInitializeCorrectly()
        {
            using var host = Host.CreateDefaultBuilder(Array.Empty<string>()).ConfigureServices(arg =>
            {
                arg.AddCli()
                .AddExtractor<MockCommandExtractor, MockArgumentExtractor>();
            }).Build();

            AssertHostServices(host);

            Assert.IsFalse(setupActionCalled);
        }

        [TestMethod]
        public void AddCliOptionsShouldInitializeCorrectly()
        {
            using var host = Host.CreateDefaultBuilder(Array.Empty<string>()).ConfigureServices(arg =>
            {
                arg.AddCli(SetupAction)
                .AddExtractor<MockCommandExtractor, MockArgumentExtractor>();
            }).Build();

            AssertHostServices(host);

            Assert.IsTrue(setupActionCalled);
        }

        private static void AssertHostServices(IHost host)
        {
            // Check Options are added
            TerminalOptions? terminalOptions = host.Services.GetService<TerminalOptions>();
            Assert.IsNotNull(terminalOptions);

            // Options is singleton
            TerminalOptions? cliOptions2 = host.Services.GetService<TerminalOptions>();
            TerminalOptions? cliOptions3 = host.Services.GetService<TerminalOptions>();
            Assert.IsTrue(ReferenceEquals(terminalOptions, cliOptions2));
            Assert.IsTrue(ReferenceEquals(terminalOptions, cliOptions3));

            // Check ICommandRouter is added as a transient
            ICommandRouter? commandRouter = host.Services.GetService<ICommandRouter>();
            Assert.IsNotNull(commandRouter);
            Assert.IsInstanceOfType(commandRouter, typeof(CommandRouter));
            Assert.IsFalse(ReferenceEquals(commandRouter, host.Services.GetService<ICommandRouter>())); // Non singleton

            // Check ICommandRouter is added as a transient
            ICommandHandler? commandHandler = host.Services.GetService<ICommandHandler>();
            Assert.IsNotNull(commandHandler);
            Assert.IsInstanceOfType(commandHandler, typeof(CommandHandler));
            Assert.IsFalse(ReferenceEquals(commandHandler, host.Services.GetService<ICommandHandler>()));

            // License checker is added as a singleton
            ILicenseChecker? licenseChecker = host.Services.GetService<ILicenseChecker>();
            Assert.IsNotNull(licenseChecker);
            Assert.IsInstanceOfType(licenseChecker, typeof(LicenseChecker));
            Assert.IsTrue(ReferenceEquals(licenseChecker, host.Services.GetService<ILicenseChecker>())); // Singleton

            // License extractor is added as a singleton
            ILicenseExtractor? licenseExtractor = host.Services.GetService<ILicenseExtractor>();
            Assert.IsNotNull(licenseExtractor);
            Assert.IsInstanceOfType(licenseExtractor, typeof(LicenseExtractor));
            Assert.IsTrue(ReferenceEquals(licenseExtractor, host.Services.GetService<ILicenseExtractor>()));
        }

        private void SetupAction(TerminalOptions obj)
        {
            setupActionCalled = true;
        }

        private bool setupActionCalled;
    }
}