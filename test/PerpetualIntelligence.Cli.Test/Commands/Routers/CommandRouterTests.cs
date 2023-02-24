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
using System.Threading;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands.Routers
{
    [TestClass]
    public class CommandRouterTests : InitializerTests
    {
        public CommandRouterTests() : base(TestLogger.Create<CommandRouterTests>())
        {
        }

        [TestMethod]
        public async Task ExtractorErrorShouldNotRouteFurtherAsync()
        {
            commandExtractor.IsExplicitError = true;

            CommandRouterContext routerContext = new("test_command_string", cancellationTokenSource.Token);

            await TestHelper.AssertThrowsErrorExceptionAsync(() => router.RouteAsync(routerContext), "test_extractor_error", "test_extractor_error_desc");
            Assert.IsTrue(commandExtractor.Called);
            Assert.IsFalse(commandHandler.Called);
        }

        [TestMethod]
        public async Task ExtractorNoExtractedCommandDescriptorShouldNotRouteFurtherAsync()
        {
            commandExtractor.IsExplicitNoCommandIdenitity = true;

            CommandRouterContext routerContext = new("test_command_string", cancellationTokenSource.Token);

            await TestHelper.AssertThrowsWithMessageAsync<ArgumentException>(() => router.RouteAsync(routerContext), "Value cannot be null. (Parameter 'commandDescriptor')");
            Assert.IsTrue(commandExtractor.Called);
            Assert.IsFalse(commandHandler.Called);
        }

        [TestMethod]
        public async Task ExtractorNoExtractedCommandShouldNotRouteFurtherAsync()
        {
            commandExtractor.IsExplicitNoCommand = true;

            CommandRouterContext routerContext = new("test_command_string", cancellationTokenSource.Token);

            await TestHelper.AssertThrowsWithMessageAsync<ArgumentException>(() => router.RouteAsync(routerContext), "Value cannot be null. (Parameter 'command')");
            Assert.IsTrue(commandExtractor.Called);
            Assert.IsFalse(commandHandler.Called);
        }

        [TestMethod]
        public async Task HandlerErrorShouldNotRouteFurtherAsync()
        {
            commandHandler.IsExplicitError = true;

            CommandRouterContext routerContext = new("test_command_string", cancellationTokenSource.Token);

            await TestHelper.AssertThrowsErrorExceptionAsync(() => router.RouteAsync(routerContext), "test_handler_error", "test_handler_error_desc");
            Assert.IsTrue(commandHandler.Called);
        }

        [TestMethod]
        public async Task RouterShouldCallExtractorAsync()
        {
            CommandRouterContext routerContext = new("test_command_string", cancellationTokenSource.Token);
            var result = await router.RouteAsync(routerContext);
            Assert.IsTrue(commandExtractor.Called);
        }

        [TestMethod]
        public async Task RouterShouldCallHandlerAfterExtractorAsync()
        {
            CommandRouterContext routerContext = new("test_command_string", cancellationTokenSource.Token);
            var result = await router.RouteAsync(routerContext); ;

            Assert.IsTrue(commandExtractor.Called);
            Assert.IsTrue(commandHandler.Called);
        }

        [TestMethod]
        public async Task RouterShouldErrorOnNoLicense()
        {
            licenseExtractor.NoLicense = true;

            CommandRouterContext routerContext = new("test_command_string", cancellationTokenSource.Token);
            await TestHelper.AssertThrowsErrorExceptionAsync(() => router.RouteAsync(routerContext), "invalid_license", "Failed to extract a valid license. Please configure the cli hosted service correctly.");

            Assert.IsFalse(commandExtractor.Called);
            Assert.IsFalse(commandHandler.Called);
        }

        [TestMethod]
        public async Task RouterShouldPassExtracterCommandToHandlerAsync()
        {
            CommandRouterContext routerContext = new("test_command_string", cancellationTokenSource.Token);
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
            licenseExtractor.NoLicense = false;

            CommandRouterContext routerContext = new("test_command_string", cancellationTokenSource.Token);
            var result = await router.RouteAsync(routerContext);

            Assert.IsNotNull(commandHandler.ContextCalled);
            Assert.AreEqual(licenseExtractor.TestLicense, commandHandler.ContextCalled.License);
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
            cancellationTokenSource = new CancellationTokenSource();
        }

        private CancellationTokenSource cancellationTokenSource;
        private MockCommandExtractorInner commandExtractor = null!;
        private MockCommandHandlerInner commandHandler = null!;
        private IHost host = null!;
        private MockLicenseExtractorInner licenseExtractor = null!;
        private CliOptions options = null!;
        private CommandRouter router = null!;
    }
}