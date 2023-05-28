/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerpetualIntelligence.Cli.Commands.Routers.Mocks;
using PerpetualIntelligence.Cli.Configuration.Options;
using PerpetualIntelligence.Cli.Mocks;
using PerpetualIntelligence.Shared.Exceptions;
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

            await TestHelper.AssertThrowsErrorExceptionAsync(() => commandRouter.RouteAsync(routerContext), "test_extractor_error", "test_extractor_error_desc");
            Assert.IsTrue(commandExtractor.Called);
            Assert.IsFalse(commandHandler.Called);
        }

        [TestMethod]
        public async Task ExtractorNoExtractedCommandDescriptorShouldNotRouteFurtherAsync()
        {
            commandExtractor.IsExplicitNoCommandDescriptor = true;

            CommandRouterContext routerContext = new("test_command_string", cancellationTokenSource.Token);

            await TestHelper.AssertThrowsWithMessageAsync<ArgumentException>(() => commandRouter.RouteAsync(routerContext), "Value cannot be null. (Parameter 'commandDescriptor')");
            Assert.IsTrue(commandExtractor.Called);
            Assert.IsFalse(commandHandler.Called);
        }

        [TestMethod]
        public async Task ExtractorNoExtractedCommandShouldNotRouteFurtherAsync()
        {
            commandExtractor.IsExplicitNoCommand = true;

            CommandRouterContext routerContext = new("test_command_string", cancellationTokenSource.Token);

            await TestHelper.AssertThrowsWithMessageAsync<ArgumentException>(() => commandRouter.RouteAsync(routerContext), "Value cannot be null. (Parameter 'command')");
            Assert.IsTrue(commandExtractor.Called);
            Assert.IsFalse(commandHandler.Called);
        }

        [TestMethod]
        public async Task HandlerErrorShouldNotRouteFurtherAsync()
        {
            commandHandler.IsExplicitError = true;

            CommandRouterContext routerContext = new("test_command_string", cancellationTokenSource.Token);

            await TestHelper.AssertThrowsErrorExceptionAsync(() => commandRouter.RouteAsync(routerContext), "test_handler_error", "test_handler_error_desc");
            Assert.IsTrue(commandHandler.Called);
        }

        [TestMethod]
        public async Task RouterShouldCallExtractorAsync()
        {
            CommandRouterContext routerContext = new("test_command_string", cancellationTokenSource.Token);
            var result = await commandRouter.RouteAsync(routerContext);
            Assert.IsTrue(commandExtractor.Called);
        }

        [TestMethod]
        public async Task RouterShouldCallHandlerAfterExtractorAsync()
        {
            CommandRouterContext routerContext = new("test_command_string", cancellationTokenSource.Token);
            var result = await commandRouter.RouteAsync(routerContext); ;

            Assert.IsTrue(commandExtractor.Called);
            Assert.IsTrue(commandHandler.Called);
        }

        [TestMethod]
        public async Task RouterShouldErrorOnNoLicense()
        {
            licenseExtractor.NoLicense = true;

            CommandRouterContext routerContext = new("test_command_string", cancellationTokenSource.Token);
            await TestHelper.AssertThrowsErrorExceptionAsync(() => commandRouter.RouteAsync(routerContext), "invalid_license", "Failed to extract a valid license. Please configure the cli hosted service correctly.");

            Assert.IsFalse(commandExtractor.Called);
            Assert.IsFalse(commandHandler.Called);
        }

        [TestMethod]
        public async Task RouterShouldPassExtracterCommandToHandlerAsync()
        {
            CommandRouterContext routerContext = new("test_command_string", cancellationTokenSource.Token);
            var result = await commandRouter.RouteAsync(routerContext);

            Assert.IsNotNull(commandHandler.ContextCalled);
            Assert.AreEqual("test_id", commandHandler.ContextCalled.Command.Descriptor.Id);
            Assert.AreEqual("test_name", commandHandler.ContextCalled.Command.Descriptor.Name);
            Assert.AreEqual("test_prefix", commandHandler.ContextCalled.Command.Descriptor.Prefix);

            Assert.AreEqual("test_id", commandHandler.ContextCalled.Command.Id);
            Assert.AreEqual("test_name", commandHandler.ContextCalled.Command.Name);
        }

        [TestMethod]
        public async Task RouterShouldPassLicenseToHandler()
        {
            licenseExtractor.NoLicense = false;

            CommandRouterContext routerContext = new("test_command_string", cancellationTokenSource.Token);
            var result = await commandRouter.RouteAsync(routerContext);

            Assert.IsNotNull(commandHandler.ContextCalled);
            Assert.AreEqual(licenseExtractor.TestLicense, commandHandler.ContextCalled.License);
            Assert.IsTrue(commandExtractor.Called);
            Assert.IsTrue(commandHandler.Called);
        }

        [TestMethod]
        public void EachContextShouldBeUnique()
        {
            CommandRouterContext context1 = new("test", CancellationToken.None);
            CommandRouterContext context2 = new("test", CancellationToken.None);
            CommandRouterContext context3 = new("test", CancellationToken.None);

            context1.Route.Id.Should().NotBe(context2.Route.Id);
            context2.Route.Id.Should().NotBe(context3.Route.Id);
            context1.Route.Id.Should().NotBe(context3.Route.Id);
        }

        [TestMethod]
        public async Task RouterShouldPassRouteToHandler()
        {
            CommandRouterContext routerContext = new("test_command_string", cancellationTokenSource.Token);
            var result = await commandRouter.RouteAsync(routerContext);

            Assert.IsNotNull(commandHandler.ContextCalled);
            commandHandler.Called.Should().BeTrue();
            commandHandler.ContextCalled.Command.Route.Id.Should().Be("id1");
        }

        [TestMethod]
        public void RouteWithNullOptionsShouldThrow()
        {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            TestHelper.AssertThrowsWithMessage<ArgumentNullException>(() => new CommandRouter(null, commandExtractor, commandHandler), "Value cannot be null. (Parameter 'licenseExtractor')");
            TestHelper.AssertThrowsWithMessage<ArgumentNullException>(() => new CommandRouter(licenseExtractor, null, commandHandler), "Value cannot be null. (Parameter 'commandExtractor')");
            TestHelper.AssertThrowsWithMessage<ArgumentNullException>(() => new CommandRouter(licenseExtractor, commandExtractor, null), "Value cannot be null. (Parameter 'commandHandler')");
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        }

        [TestMethod]
        public async Task ShouldCallRouterEventIfConfigured()
        {
            eventHandler.BeforeRouteCalled.Should().BeFalse();
            eventHandler.AfterRouteCalled.Should().BeFalse();

            CommandRouterContext routerContext = new("test_command_string", cancellationTokenSource.Token);
            var result = await commandRouter.RouteAsync(routerContext);

            eventHandler.BeforeRouteCalled.Should().BeTrue();
            eventHandler.AfterRouteCalled.Should().BeTrue();

            eventHandler.PassedRoute.Should().NotBeNull();
            eventHandler.PassedCommand.Should().NotBeNull();
            eventHandler.PassedRouterResult.Should().NotBeNull();

            eventHandler.PassedRoute.Command.Raw.Should().Be("test_command_string");
        }

        [TestMethod]
        public async Task ShouldCallRouterEventInCaseOfExceptionIfConfigured()
        {
            eventHandler.BeforeRouteCalled.Should().BeFalse();
            eventHandler.AfterRouteCalled.Should().BeFalse();

            commandExtractor.IsExplicitError = true;

            try
            {
                CommandRouterContext routerContext = new("test_command_string", cancellationTokenSource.Token);
                var result = await commandRouter.RouteAsync(routerContext);
            }
            catch (ErrorException eex)
            {
                eex.Error.ErrorCode.Should().Be("test_extractor_error");
                eex.Error.ErrorDescription.Should().Be("test_extractor_error_desc");
            }

            // Event fired even with exception
            eventHandler.BeforeRouteCalled.Should().BeTrue();
            eventHandler.AfterRouteCalled.Should().BeTrue();

            eventHandler.PassedRoute.Should().NotBeNull();
            eventHandler.PassedCommand.Should().BeNull();
            eventHandler.PassedRouterResult.Should().BeNull();

            eventHandler.PassedRoute.Command.Raw.Should().Be("test_command_string");
        }

        [TestMethod]
        public async Task ShouldNotCallRouterEventIfNotConfigured()
        {
            eventHandler.BeforeRouteCalled.Should().BeFalse();
            eventHandler.AfterRouteCalled.Should().BeFalse();

            commandRouter = new CommandRouter(licenseExtractor, commandExtractor, commandHandler, asyncEventHandler: null);
            CommandRouterContext routerContext = new("test_command_string", cancellationTokenSource.Token);
            var result = await commandRouter.RouteAsync(routerContext);

            eventHandler.BeforeRouteCalled.Should().BeFalse();
            eventHandler.AfterRouteCalled.Should().BeFalse();

            eventHandler.PassedRoute.Should().BeNull();
            eventHandler.PassedCommand.Should().BeNull();
            eventHandler.PassedRouterResult.Should().BeNull();
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
            eventHandler = new MockAsyncEventHandler();
            commandRouter = new CommandRouter(licenseExtractor, commandExtractor, commandHandler, eventHandler);
            cancellationTokenSource = new CancellationTokenSource();
        }

        private CancellationTokenSource cancellationTokenSource = null!;
        private MockCommandExtractorInner commandExtractor = null!;
        private MockCommandHandlerInner commandHandler = null!;
        private MockAsyncEventHandler eventHandler = null!;
        private IHost host = null!;
        private MockLicenseExtractorInner licenseExtractor = null!;
        private CliOptions options = null!;
        private CommandRouter commandRouter = null!;
    }
}