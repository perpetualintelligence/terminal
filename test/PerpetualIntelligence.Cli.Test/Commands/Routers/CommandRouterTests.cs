/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerpetualIntelligence.Cli.Commands.Routers.Mocks;
using PerpetualIntelligence.Cli.Configuration.Options;
using PerpetualIntelligence.Cli.Mocks;
using PerpetualIntelligence.Test;
using PerpetualIntelligence.Test.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands.Routers
{
    [TestClass]
    public class CommandRouterTests : LogTest
    {
        public CommandRouterTests() : base(TestLogger.Create<CommandRouterTests>())
        {
        }

        [TestMethod]
        public async Task ExtractorErrorShouldNotRouteFurtherAsync()
        {
            commandExtractor.IsExplicitError = true;

            CommandRouterContext routerContext = new("test_command_string");

            await TestHelper.AssertThrowsErrorExceptionAsync(() => router.RouteAsync(routerContext), "test_extractor_error", "test_extractor_error_desc");
            Assert.IsTrue(commandExtractor.Called);
            Assert.IsFalse(commandHandler.Called);
        }

        [TestMethod]
        public async Task ExtractorNoExtractedCommandDescriptorShouldNotRouteFurtherAsync()
        {
            commandExtractor.IsExplicitNoCommandIdenitity = true;

            CommandRouterContext routerContext = new("test_command_string");

            await TestHelper.AssertThrowsWithMessageAsync<ArgumentException>(() => router.RouteAsync(routerContext), "Value cannot be null. (Parameter 'commandDescriptor')");
            Assert.IsTrue(commandExtractor.Called);
            Assert.IsFalse(commandHandler.Called);
        }

        [TestMethod]
        public async Task ExtractorNoExtractedCommandShouldNotRouteFurtherAsync()
        {
            commandExtractor.IsExplicitNoCommand = true;

            CommandRouterContext routerContext = new("test_command_string");

            await TestHelper.AssertThrowsWithMessageAsync<ArgumentException>(() => router.RouteAsync(routerContext), "Value cannot be null. (Parameter 'command')");
            Assert.IsTrue(commandExtractor.Called);
            Assert.IsFalse(commandHandler.Called);
        }

        [TestMethod]
        public async Task HandlerErrorShouldNotRouteFurtherAsync()
        {
            commandHandler.IsExplicitError = true;

            CommandRouterContext routerContext = new("test_command_string");

            await TestHelper.AssertThrowsErrorExceptionAsync(() => router.RouteAsync(routerContext), "test_handler_error", "test_handler_error_desc");
            Assert.IsTrue(commandHandler.Called);
        }

        [TestMethod]
        public async Task RouterShouldCallExtractorAsync()
        {
            CommandRouterContext routerContext = new("test_command_string");
            var result = await router.RouteAsync(routerContext);
            Assert.IsTrue(commandExtractor.Called);
        }

        [TestMethod]
        public async Task RouterShouldCallHandlerAfterExtractorAsync()
        {
            CommandRouterContext routerContext = new("test_command_string");
            var result = await router.RouteAsync(routerContext); ;

            Assert.IsTrue(commandExtractor.Called);
            Assert.IsTrue(commandHandler.Called);
        }

        [TestMethod]
        public async Task RouterShouldErrorOnMultipleLicense()
        {
            licenseExtractor.UseMultiple = true;

            CommandRouterContext routerContext = new("test_command_string");
            await TestHelper.AssertThrowsErrorExceptionAsync(() => router.RouteAsync(routerContext), "invalid_configuration", "The license extractor found multiple licenses.");

            Assert.IsFalse(commandExtractor.Called);
            Assert.IsFalse(commandHandler.Called);
        }

        [TestMethod]
        public async Task RouterShouldErrorOnNoLicense()
        {
            licenseExtractor.NoLicense = true;

            CommandRouterContext routerContext = new("test_command_string");
            await TestHelper.AssertThrowsErrorExceptionAsync(() => router.RouteAsync(routerContext), "invalid_configuration", "The license extractor did not find any valid license.");

            Assert.IsFalse(commandExtractor.Called);
            Assert.IsFalse(commandHandler.Called);
        }

        [TestMethod]
        public async Task RouterShouldFindHandler()
        {
            CommandRouterContext routerContext = new("test_command_string");
            var result = await router.TryFindHandlerAsync(routerContext);

            Assert.IsFalse(result.IsError);
            Assert.IsNotNull(result.Result);
            Assert.IsInstanceOfType(result.Result, typeof(MockCommandHandlerInner));
            Assert.IsTrue(ReferenceEquals(commandHandler, result.Result));
            Assert.IsFalse(commandHandler.Called);
        }

        [TestMethod]
        public async Task RouterShouldPassExtracterCommandToHandlerAsync()
        {
            CommandRouterContext routerContext = new("test_command_string");
            var result = await router.RouteAsync(routerContext);

            Assert.IsNotNull(commandHandler.ContextCalled);
            Assert.AreEqual("test_id", commandHandler.ContextCalled.CommandDescriptor.Id);
            Assert.AreEqual("test_name", commandHandler.ContextCalled.CommandDescriptor.Name);
            Assert.AreEqual("test_prefix", commandHandler.ContextCalled.CommandDescriptor.Prefix);

            Assert.AreEqual("test_id", commandHandler.ContextCalled.Command.Id);
            Assert.AreEqual("test_name", commandHandler.ContextCalled.Command.Name);
        }

        [TestMethod]
        public async Task RouterShouldPassLicenseToHandler()
        {
            licenseExtractor.UseMultiple = false;
            licenseExtractor.NoLicense = false;

            CommandRouterContext routerContext = new("test_command_string");
            var result = await router.RouteAsync(routerContext);

            Assert.IsNotNull(commandHandler.ContextCalled);
            CollectionAssert.AreEqual(licenseExtractor.SingleLicense, commandHandler.ContextCalled.Licenses.ToArray());
            Assert.IsTrue(commandExtractor.Called);
            Assert.IsTrue(commandHandler.Called);
        }

        [TestMethod]
        public void RouteWithNullArgumentsShouldThrow()
        {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            TestHelper.AssertThrowsWithMessage<ArgumentNullException>(() => new CommandRouter(null, commandExtractor, commandHandler), "Value cannot be null. (Parameter 'licenseExtractor')");
            TestHelper.AssertThrowsWithMessage<ArgumentNullException>(() => new CommandRouter(licenseExtractor, null, commandHandler), "Value cannot be null. (Parameter 'commandExtractor')");
            TestHelper.AssertThrowsWithMessage<ArgumentNullException>(() => new CommandRouter(licenseExtractor, commandExtractor, null), "Value cannot be null. (Parameter 'commandHandler')");
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        }

        protected override void OnTestCleanup()
        {
            if (host != null)
            {
                host.Dispose();
            }
        }

        protected override void OnTestInitialize()
        {
            var hostBuilder = Host.CreateDefaultBuilder(Array.Empty<string>());
            host = hostBuilder.Build();

            options = MockCliOptions.New();
            commandExtractor = new MockCommandExtractorInner();
            commandHandler = new MockCommandHandlerInner();
            licenseExtractor = new MockLicenseExtractorInner();
            router = new CommandRouter(licenseExtractor, commandExtractor, commandHandler);
        }

        private MockCommandExtractorInner commandExtractor = null!;
        private MockCommandHandlerInner commandHandler = null!;
        private IHost host = null!;
        private MockLicenseExtractorInner licenseExtractor = null!;
        private CliOptions options = null!;
        private CommandRouter router = null!;
    }
}
