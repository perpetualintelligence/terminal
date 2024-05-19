/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OneImlx.Terminal.Commands.Routers.Mocks;
using OneImlx.Terminal.Configuration.Options;
using OneImlx.Terminal.Mocks;
using OneImlx.Test.FluentAssertions;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace OneImlx.Terminal.Commands.Routers
{
    public class CommandRouterTests : IAsyncLifetime
    {
        [Fact]
        public async Task ExtractorErrorShouldNotRouteFurtherAsync()
        {
            commandParser.IsExplicitError = true;

            CommandRouterContext routerContext = new("test_command_string", routingContext, null);
            Func<Task> func = async () => await commandRouter.RouteCommandAsync(routerContext);
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode("test_parser_error").WithErrorDescription("test_parser_error_desc");
            commandParser.Called.Should().BeTrue();
            commandHandler.Called.Should().BeFalse();
        }

        [Fact]
        public async Task ExtractorNoExtractedCommandDescriptorShouldNotRouteFurtherAsync()
        {
            commandParser.IsExplicitNoCommandDescriptor = true;

            CommandRouterContext routerContext = new("test_command_string", routingContext, null);

            Func<Task> act = async () => await commandRouter.RouteCommandAsync(routerContext);
            await act.Should().ThrowAsync<ArgumentException>();
            commandParser.Called.Should().BeTrue();
            commandHandler.Called.Should().BeFalse();
        }

        [Fact]
        public async Task ExtractorNoExtractedCommandShouldNotRouteFurtherAsync()
        {
            commandParser.IsExplicitNoExtractedCommand = true;

            CommandRouterContext routerContext = new("test_command_string", routingContext, null);

            Func<Task> act = async () => await commandRouter.RouteCommandAsync(routerContext);
            await act.Should().ThrowAsync<ArgumentException>();
            commandParser.Called.Should().BeTrue();
            commandHandler.Called.Should().BeFalse();
        }

        [Fact]
        public async Task HandlerErrorShouldNotRouteFurtherAsync()
        {
            commandHandler.IsExplicitError = true;

            CommandRouterContext routerContext = new("test_command_string", routingContext, null);
            Func<Task> func = async () => await commandRouter.RouteCommandAsync(routerContext);
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode("test_handler_error").WithErrorDescription("test_handler_error_desc");
            commandHandler.Called.Should().BeTrue();
        }

        [Fact]
        public async Task RouterShouldCallExtractorAsync()
        {
            CommandRouterContext routerContext = new("test_command_string", routingContext, null);
            await commandRouter.RouteCommandAsync(routerContext);
            commandParser.Called.Should().BeTrue();
        }

        [Fact]
        public async Task RouterShouldCallHandlerAfterExtractorAsync()
        {
            CommandRouterContext routerContext = new("test_command_string", routingContext, null);
            await commandRouter.RouteCommandAsync(routerContext); ;

            commandParser.Called.Should().BeTrue();
            commandHandler.Called.Should().BeTrue();
        }

        [Fact]
        public async Task RouterShouldErrorOnNoLicense()
        {
            licenseExtractor.NoLicense = true;

            CommandRouterContext routerContext = new("test_command_string", routingContext, null);
            Func<Task> func = async () => await commandRouter.RouteCommandAsync(routerContext);
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode("invalid_license").WithErrorDescription("Failed to extract a valid license. Please configure the hosted service correctly.");

            commandParser.Called.Should().BeFalse();
            commandHandler.Called.Should().BeFalse();
        }

        [Fact]
        public async Task RouterShouldPassExtractorCommandToHandlerAsync()
        {
            CommandRouterContext routerContext = new("test_command_string", routingContext, null);
            await commandRouter.RouteCommandAsync(routerContext);

            commandHandler.ContextCalled.Should().NotBeNull();
            commandHandler.ContextCalled!.ParsedCommand.Command.Descriptor.Id.Should().Be("test_id");
            commandHandler.ContextCalled.ParsedCommand.Command.Descriptor.Name.Should().Be("test_name");
        }

        [Fact]
        public async Task RouterShouldPassLicenseToHandler()
        {
            licenseExtractor.NoLicense = false;

            CommandRouterContext routerContext = new("test_command_string", routingContext, null);
            await commandRouter.RouteCommandAsync(routerContext);

            commandHandler.ContextCalled.Should().NotBeNull();
            licenseExtractor.TestLicense.Should().BeSameAs(commandHandler.ContextCalled!.License);
            commandParser.Called.Should().BeTrue();
            commandHandler.Called.Should().BeTrue();
        }

        [Fact]
        public void EachContextShouldBeUnique()
        {
            CommandRouterContext context1 = new("test", routingContext, null);
            CommandRouterContext context2 = new("test", routingContext, null);
            CommandRouterContext context3 = new("test", routingContext, null);

            context1.Route.Id.Should().NotBe(context2.Route.Id);
            context2.Route.Id.Should().NotBe(context3.Route.Id);
            context1.Route.Id.Should().NotBe(context3.Route.Id);
        }

        [Fact]
        public void RouteWithNullOptionsShouldThrow()
        {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CA1806 // Do not ignore method results
            Action act = () => new CommandRouter(null, licenseExtractor, commandParser, commandHandler, logger);
            act.Should().Throw<ArgumentNullException>();

            act = () => new CommandRouter(terminalOptions, null, commandParser, commandHandler, logger);
            act.Should().Throw<ArgumentNullException>();

            act = () => new CommandRouter(terminalOptions, licenseExtractor, null, commandHandler, logger);
            act.Should().Throw<ArgumentNullException>();

            act = () => new CommandRouter(terminalOptions, licenseExtractor, commandParser, null, logger);
            act.Should().Throw<ArgumentNullException>();
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type
#pragma warning restore CA1806 // Do not ignore method results
        }

        [Fact]
        public async Task ShouldCallRouterEventIfConfigured()
        {
            eventHandler.BeforeRouteCalled.Should().BeFalse();
            eventHandler.AfterRouteCalled.Should().BeFalse();

            CommandRouterContext routerContext = new("test_command_string", routingContext, null);
            await commandRouter.RouteCommandAsync(routerContext);

            eventHandler.BeforeRouteCalled.Should().BeTrue();
            eventHandler.AfterRouteCalled.Should().BeTrue();

            eventHandler.PassedRoute.Should().NotBeNull();
            eventHandler.PassedRoute!.Raw.Should().Be("test_command_string");

            eventHandler.PassedCommand.Should().NotBeNull();
            eventHandler.PassedRouterResult.Should().NotBeNull();
        }

        [Fact]
        public async Task ShouldCallRouterEventInCaseOfExceptionIfConfigured()
        {
            eventHandler.BeforeRouteCalled.Should().BeFalse();
            eventHandler.AfterRouteCalled.Should().BeFalse();

            commandParser.IsExplicitError = true;

            try
            {
                CommandRouterContext routerContext = new("test_command_string", routingContext, null);
                var result = await commandRouter.RouteCommandAsync(routerContext);
            }
            catch (TerminalException eex)
            {
                eex.Error.ErrorCode.Should().Be("test_parser_error");
                eex.Error.ErrorDescription.Should().Be("test_parser_error_desc");
            }

            // Event fired even with exception
            eventHandler.BeforeRouteCalled.Should().BeTrue();
            eventHandler.AfterRouteCalled.Should().BeTrue();

            eventHandler.PassedRoute.Should().NotBeNull();
            eventHandler.PassedRoute!.Raw.Should().Be("test_command_string");

            eventHandler.PassedCommand.Should().BeNull();
            eventHandler.PassedRouterResult.Should().BeNull();
        }

        [Fact]
        public async Task ShouldNotCallRouterEventIfNotConfigured()
        {
            eventHandler.BeforeRouteCalled.Should().BeFalse();
            eventHandler.AfterRouteCalled.Should().BeFalse();

            commandRouter = new CommandRouter(terminalOptions, licenseExtractor, commandParser, commandHandler, logger, asyncEventHandler: null);
            CommandRouterContext routerContext = new("test_command_string", routingContext, null);
            await commandRouter.RouteCommandAsync(routerContext);

            eventHandler.BeforeRouteCalled.Should().BeFalse();
            eventHandler.AfterRouteCalled.Should().BeFalse();

            eventHandler.PassedRoute.Should().BeNull();
            eventHandler.PassedCommand.Should().BeNull();
            eventHandler.PassedRouterResult.Should().BeNull();
        }

        public Task InitializeAsync()
        {
            var hostBuilder = Host.CreateDefaultBuilder([]);
            host = hostBuilder.Build();

            commandParser = new MockCommandParserInner();
            commandHandler = new MockCommandHandlerInner();
            licenseExtractor = new MockLicenseExtractorInner();
            eventHandler = new MockTerminalEventHandler();
            terminalOptions = MockTerminalOptions.NewLegacyOptions();
            logger = new LoggerFactory().CreateLogger<CommandRouter>();
            commandRouter = new CommandRouter(terminalOptions, licenseExtractor, commandParser, commandHandler, logger, eventHandler);
            terminalTokenSource = new CancellationTokenSource();
            commandTokenSource = new CancellationTokenSource();
            routingContext = new MockTerminalRouterContext(new Runtime.TerminalStartContext(Runtime.TerminalStartMode.Custom, terminalTokenSource.Token, commandTokenSource.Token));

            return Task.CompletedTask;
        }

        public Task DisposeAsync()
        {
            host?.Dispose();

            return Task.CompletedTask;
        }

        private CancellationTokenSource terminalTokenSource = null!;
        private CancellationTokenSource commandTokenSource = null!;
        private MockCommandParserInner commandParser = null!;
        private MockCommandHandlerInner commandHandler = null!;
        private MockTerminalEventHandler eventHandler = null!;
        private MockTerminalRouterContext routingContext = null!;
        private IHost host = null!;
        private MockLicenseExtractorInner licenseExtractor = null!;
        private CommandRouter commandRouter = null!;
        private TerminalOptions terminalOptions = null!;
        private ILogger<CommandRouter> logger = null!;
    }
}