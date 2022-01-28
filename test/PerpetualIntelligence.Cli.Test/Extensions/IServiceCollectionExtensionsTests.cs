/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerpetualIntelligence.Cli.Commands.Handlers;
using PerpetualIntelligence.Cli.Commands.Routers;
using PerpetualIntelligence.Cli.Configuration.Options;
using PerpetualIntelligence.Cli.Integration;
using PerpetualIntelligence.Cli.Mocks;
using PerpetualIntelligence.Test;
using PerpetualIntelligence.Test.Services;
using System;
using System.Collections.Generic;

namespace PerpetualIntelligence.Cli.Extensions
{
    [TestClass]
    public class IServiceCollectionExtensionsTests : LogTest
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
            ICliBuilder? cliBuilder = serviceDescriptors.AddCliBuilder();
            Assert.IsNotNull(cliBuilder);
            Assert.IsInstanceOfType(cliBuilder, typeof(CliBuilder));
            Assert.IsTrue(ReferenceEquals(serviceDescriptors, cliBuilder.Services));

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
                .AddInMemoryCollection(myConfiguration)
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
            CliOptions? cliOptions = host.Services.GetService<CliOptions>();
            Assert.IsNotNull(cliOptions);

            // Options is singleton
            CliOptions? cliOptions2 = host.Services.GetService<CliOptions>();
            CliOptions? cliOptions3 = host.Services.GetService<CliOptions>();
            Assert.IsTrue(ReferenceEquals(cliOptions, cliOptions2));
            Assert.IsTrue(ReferenceEquals(cliOptions, cliOptions3));

            // Check ICommandRouter is added as a transient
            ICommandRouter? commandRouter = host.Services.GetService<ICommandRouter>();
            Assert.IsNotNull(commandRouter);
            Assert.IsInstanceOfType(commandRouter, typeof(CommandRouter));
            Assert.IsFalse(ReferenceEquals(commandRouter, host.Services.GetService<ICommandRouter>()));

            // Check ICommandRouter is added as a transient
            ICommandHandler? commandHandler = host.Services.GetService<ICommandHandler>();
            Assert.IsNotNull(commandHandler);
            Assert.IsInstanceOfType(commandHandler, typeof(CommandHandler));
            Assert.IsFalse(ReferenceEquals(commandHandler, host.Services.GetService<ICommandHandler>()));
        }

        private void SetupAction(CliOptions obj)
        {
            setupActionCalled = true;
        }

        private bool setupActionCalled;
    }
}
