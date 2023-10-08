/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerpetualIntelligence.Terminal.Commands.Routers.Mocks;
using PerpetualIntelligence.Terminal.Configuration.Options;
using PerpetualIntelligence.Terminal.Mocks;
using PerpetualIntelligence.Test;
using PerpetualIntelligence.Test.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Commands.Routers
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

            CommandRouterContext routerContext = new("test_command_string", routingContext);

            await TestHelper.AssertThrowsErrorExceptionAsync<TerminalException>(() => commandRouter.RouteCommandAsync(routerContext), "test_extractor_error", "test_extractor_error_desc");
            Assert.IsTrue(commandExtractor.Called);
            Assert.IsFalse(commandHandler.Called);
        }

        [TestMethod]
        public async Task ExtractorNoExtractedCommandDescriptorShouldNotRouteFurtherAsync()
        {
            commandExtractor.IsExplicitNoCommandDescriptor = true;

            CommandRouterContext routerContext = new("test_command_string", routingContext);

            Func<Task> act = async () => await commandRouter.RouteCommandAsync(routerContext);
            await act.Should().ThrowAsync<ArgumentException>().WithMessage("Value cannot be null. (Parameter 'commandDescriptor')");
            Assert.IsTrue(commandExtractor.Called);
            Assert.IsFalse(commandHandler.Called);
        }

        [TestMethod]
        public async Task ExtractorNoExtractedCommandShouldNotRouteFurtherAsync()
        {
            commandExtractor.IsExplicitNoExtractedCommand = true;

            CommandRouterContext routerContext = new("test_command_string", routingContext);

            Func<Task> act = async () => await commandRouter.RouteCommandAsync(routerContext);
            await act.Should().ThrowAsync<ArgumentException>().WithMessage("Value cannot be null. (Parameter 'parsedCommand')");
            Assert.IsTrue(commandExtractor.Called);
            Assert.IsFalse(commandHandler.Called);
        }

        [TestMethod]
        public async Task HandlerErrorShouldNotRouteFurtherAsync()
        {
            commandHandler.IsExplicitError = true;

            CommandRouterContext routerContext = new("test_command_string", routingContext);

            await TestHelper.AssertThrowsErrorExceptionAsync<TerminalException>(() => commandRouter.RouteCommandAsync(routerContext), "test_handler_error", "test_handler_error_desc");
            Assert.IsTrue(commandHandler.Called);
        }

        [TestMethod]
        public async Task RouterShouldCallExtractorAsync()
        {
            CommandRouterContext routerContext = new("test_command_string", routingContext);
            await commandRouter.RouteCommandAsync(routerContext);
            Assert.IsTrue(commandExtractor.Called);
        }

        [TestMethod]
        public async Task RouterShouldCallHandlerAfterExtractorAsync()
        {
            CommandRouterContext routerContext = new("test_command_string", routingContext);
            await commandRouter.RouteCommandAsync(routerContext); ;

            Assert.IsTrue(commandExtractor.Called);
            Assert.IsTrue(commandHandler.Called);
        }

        [TestMethod]
        public async Task RouterShouldErrorOnNoLicense()
        {
            licenseExtractor.NoLicense = true;

            CommandRouterContext routerContext = new("test_command_string", routingContext);
            await TestHelper.AssertThrowsErrorExceptionAsync<TerminalException>(() => commandRouter.RouteCommandAsync(routerContext), "invalid_license", "Failed to extract a valid license. Please configure the cli hosted service correctly.");

            Assert.IsFalse(commandExtractor.Called);
            Assert.IsFalse(commandHandler.Called);
        }

        [TestMethod]
        public async Task RouterShouldPassExtractorCommandToHandlerAsync()
        {
            CommandRouterContext routerContext = new("test_command_string", routingContext);
            await commandRouter.RouteCommandAsync(routerContext);

            Assert.IsNotNull(commandHandler.ContextCalled);
            Assert.AreEqual("test_id", commandHandler.ContextCalled.ParsedCommand.Command.Descriptor.Id);
            Assert.AreEqual("test_name", commandHandler.ContextCalled.ParsedCommand.Command.Descriptor.Name);

            Assert.AreEqual("test_id", commandHandler.ContextCalled.ParsedCommand.Command.Id);
            Assert.AreEqual("test_name", commandHandler.ContextCalled.ParsedCommand.Command.Name);
        }

        [TestMethod]
        public async Task RouterShouldPassLicenseToHandler()
        {
            licenseExtractor.NoLicense = false;

            CommandRouterContext routerContext = new("test_command_string", routingContext);
            await commandRouter.RouteCommandAsync(routerContext);

            Assert.IsNotNull(commandHandler.ContextCalled);
            Assert.AreEqual(licenseExtractor.TestLicense, commandHandler.ContextCalled.License);
            Assert.IsTrue(commandExtractor.Called);
            Assert.IsTrue(commandHandler.Called);
        }

        [TestMethod]
        public void EachContextShouldBeUnique()
        {
            CommandRouterContext context1 = new("test", routingContext);
            CommandRouterContext context2 = new("test", routingContext);
            CommandRouterContext context3 = new("test", routingContext);

            context1.Route.Id.Should().NotBe(context2.Route.Id);
            context2.Route.Id.Should().NotBe(context3.Route.Id);
            context1.Route.Id.Should().NotBe(context3.Route.Id);
        }

        [TestMethod]
        public void RouteWithNullOptionsShouldThrow()
        {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CA1806 // Do not ignore method results
            Action act = () => new CommandRouter(null, licenseExtractor, commandExtractor, commandHandler);
            act.Should().Throw<ArgumentNullException>().WithMessage("Value cannot be null. (Parameter 'terminalOptions')");

            act = () => new CommandRouter(terminalOptions, null, commandExtractor, commandHandler);
            act.Should().Throw<ArgumentNullException>().WithMessage("Value cannot be null. (Parameter 'licenseExtractor')");

            act = () => new CommandRouter(terminalOptions, licenseExtractor, null, commandHandler);
            act.Should().Throw<ArgumentNullException>().WithMessage("Value cannot be null. (Parameter 'commandExtractor')");

            act = () => new CommandRouter(terminalOptions, licenseExtractor, commandExtractor, null);
            act.Should().Throw<ArgumentNullException>().WithMessage("Value cannot be null. (Parameter 'commandHandler')");
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type
#pragma warning restore CA1806 // Do not ignore method results
        }

        [TestMethod]
        public async Task ShouldCallRouterEventIfConfigured()
        {
            eventHandler.BeforeRouteCalled.Should().BeFalse();
            eventHandler.AfterRouteCalled.Should().BeFalse();

            CommandRouterContext routerContext = new("test_command_string", routingContext);
            await commandRouter.RouteCommandAsync(routerContext);

            eventHandler.BeforeRouteCalled.Should().BeTrue();
            eventHandler.AfterRouteCalled.Should().BeTrue();

            eventHandler.PassedRoute.Should().NotBeNull();
            eventHandler.PassedRoute!.Command.Raw.Should().Be("test_command_string");

            eventHandler.PassedCommand.Should().NotBeNull();
            eventHandler.PassedRouterResult.Should().NotBeNull();
        }

        [TestMethod]
        public async Task ShouldCallRouterEventInCaseOfExceptionIfConfigured()
        {
            eventHandler.BeforeRouteCalled.Should().BeFalse();
            eventHandler.AfterRouteCalled.Should().BeFalse();

            commandExtractor.IsExplicitError = true;

            try
            {
                CommandRouterContext routerContext = new("test_command_string", routingContext);
                var result = await commandRouter.RouteCommandAsync(routerContext);
            }
            catch (TerminalException eex)
            {
                eex.Error.ErrorCode.Should().Be("test_extractor_error");
                eex.Error.ErrorDescription.Should().Be("test_extractor_error_desc");
            }

            // Event fired even with exception
            eventHandler.BeforeRouteCalled.Should().BeTrue();
            eventHandler.AfterRouteCalled.Should().BeTrue();

            eventHandler.PassedRoute.Should().NotBeNull();
            eventHandler.PassedRoute!.Command.Raw.Should().Be("test_command_string");

            eventHandler.PassedCommand.Should().BeNull();
            eventHandler.PassedRouterResult.Should().BeNull();
        }

        [TestMethod]
        public async Task ShouldNotCallRouterEventIfNotConfigured()
        {
            eventHandler.BeforeRouteCalled.Should().BeFalse();
            eventHandler.AfterRouteCalled.Should().BeFalse();

            commandRouter = new CommandRouter(terminalOptions, licenseExtractor, commandExtractor, commandHandler, asyncEventHandler: null);
            CommandRouterContext routerContext = new("test_command_string", routingContext);
            await commandRouter.RouteCommandAsync(routerContext);

            eventHandler.BeforeRouteCalled.Should().BeFalse();
            eventHandler.AfterRouteCalled.Should().BeFalse();

            eventHandler.PassedRoute.Should().BeNull();
            eventHandler.PassedCommand.Should().BeNull();
            eventHandler.PassedRouterResult.Should().BeNull();
        }

        [TestMethod]
        public async Task Router_Throws_On_CommandString_MaxLimitAsync()
        {
            terminalOptions.Router.MaxMessageLength = 30;
            Func<Task> act = () => commandRouter.RouteCommandAsync(new CommandRouterContext(new string('x', 31), routingContext));
            await act.Should().ThrowAsync<TerminalException>().WithMessage("The command string length is over the configured limit. max_length=30");
        }

        protected override void OnTestCleanup()
        {
            host?.Dispose();
        }

        protected override void OnTestInitialize()
        {
            var hostBuilder = Host.CreateDefaultBuilder(Array.Empty<string>());
            host = hostBuilder.Build();

            commandExtractor = new MockCommandExtractorInner();
            commandHandler = new MockCommandHandlerInner();
            licenseExtractor = new MockLicenseExtractorInner();
            eventHandler = new MockAsyncEventHandler();
            terminalOptions = MockTerminalOptions.NewLegacyOptions();
            commandRouter = new CommandRouter(terminalOptions, licenseExtractor, commandExtractor, commandHandler, eventHandler);
            cancellationTokenSource = new CancellationTokenSource();
            routingContext = new MockTerminalRoutingContext(new Runtime.TerminalStartContext(new Runtime.TerminalStartInfo(Runtime.TerminalStartMode.Custom), cancellationTokenSource.Token));
        }

        private CancellationTokenSource cancellationTokenSource = null!;
        private MockCommandExtractorInner commandExtractor = null!;
        private MockCommandHandlerInner commandHandler = null!;
        private MockAsyncEventHandler eventHandler = null!;
        private MockTerminalRoutingContext routingContext = null!;
        private IHost host = null!;
        private MockLicenseExtractorInner licenseExtractor = null!;
        private CommandRouter commandRouter = null!;
        private TerminalOptions terminalOptions = null!;
    }
}