﻿/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerpetualIntelligence.Cli.Commands;
using PerpetualIntelligence.Cli.Commands.Checkers;
using PerpetualIntelligence.Cli.Commands.Extractors;
using PerpetualIntelligence.Cli.Commands.Handlers;
using PerpetualIntelligence.Cli.Commands.Providers;
using PerpetualIntelligence.Cli.Commands.Publishers;
using PerpetualIntelligence.Cli.Commands.Routers;
using PerpetualIntelligence.Cli.Commands.Stores;
using PerpetualIntelligence.Cli.Configuration.Options;
using PerpetualIntelligence.Cli.Integration;
using PerpetualIntelligence.Cli.Mocks;
using PerpetualIntelligence.Test;
using PerpetualIntelligence.Test.Services;
using System;
using System.Linq;

namespace PerpetualIntelligence.Cli.Extensions
{
    [TestClass]
    public class ICliBuilderExtensionsTests : LogTest
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
        public void AddCommandDescriptorShouldCorrectlyInitialize()
        {
            cliBuilder.AddDescriptor<MockCommandRunner, MockCommandChecker>(new CommandDescriptor("id1", "name1", "prefix1"));

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
            cliBuilder.AddDescriptorStore<MockCommandDescriptorStore>();

            var serviceDescriptor = cliBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(ICommandDescriptorStore)));
            Assert.IsNotNull(serviceDescriptor);
            Assert.AreEqual(ServiceLifetime.Transient, serviceDescriptor.Lifetime);
            Assert.AreEqual(typeof(MockCommandDescriptorStore), serviceDescriptor.ImplementationType);
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
            cliBuilder.AddPublisher<MockErrorPublisher, MockExceptionPublisher>();

            var err = cliBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(IErrorPublisher)));
            Assert.IsNotNull(err);
            Assert.AreEqual(ServiceLifetime.Transient, err.Lifetime);
            Assert.AreEqual(typeof(MockErrorPublisher), err.ImplementationType);

            var exe = cliBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(IExceptionPublisher)));
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
