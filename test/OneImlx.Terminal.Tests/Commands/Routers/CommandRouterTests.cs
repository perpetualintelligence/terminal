/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OneImlx.Terminal.Commands.Routers.Mocks;
using OneImlx.Terminal.Configuration.Options;
using OneImlx.Terminal.Mocks;
using OneImlx.Terminal.Runtime;
using OneImlx.Test.FluentAssertions;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace OneImlx.Terminal.Commands.Routers
{
    public class CommandRouterTests : IAsyncLifetime
    {
        public Task DisposeAsync()
        {
            host?.Dispose();

            return Task.CompletedTask;
        }

        [Fact]
        public async Task ExtractorErrorShouldNotRouteFurtherAsync()
        {
            commandParser.SetExplicitError = true;

            CommandContext routerContext = new(new(Guid.NewGuid().ToString(), "test_command_string"), routingContext, null);
            Func<Task> func = async () => await commandRouter.RouteCommandAsync(routerContext);
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode("test_parser_error").WithErrorDescription("test_parser_error_desc");
            commandParser.Called.Should().BeTrue();
            commandHandler.Called.Should().BeFalse();
        }

        [Fact]
        public async Task ExtractorNoExtractedCommandDescriptorShouldNotRouteFurtherAsync()
        {
            commandParser.DoNotSetCommandDescriptor = true;

            CommandContext routerContext = new(new(Guid.NewGuid().ToString(), "test_command_string"), routingContext, null);

            Func<Task> act = async () => await commandRouter.RouteCommandAsync(routerContext);
            await act.Should().ThrowAsync<ArgumentException>();
            commandParser.Called.Should().BeTrue();
            commandHandler.Called.Should().BeFalse();
        }

        [Fact]
        public async Task HandlerErrorShouldNotRouteFurtherAsync()
        {
            commandHandler.IsExplicitError = true;

            CommandContext routerContext = new(new(Guid.NewGuid().ToString(), "test_command_string"), routingContext, null);
            Func<Task> func = async () => await commandRouter.RouteCommandAsync(routerContext);
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode("test_handler_error").WithErrorDescription("test_handler_error_desc");
            commandHandler.Called.Should().BeTrue();
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
            routingContext = new MockTerminalRouterContext(TerminalStartMode.Custom, terminalTokenSource.Token, commandTokenSource.Token);

            return Task.CompletedTask;
        }

        [Fact]
        public async Task ParserFails_But_Invokes_Handler()
        {
            commandParser.DoNotSetParsedCommand = true;

            commandParser.Called.Should().BeFalse();
            commandHandler.Called.Should().BeFalse();

            CommandContext routerContext = new(new(Guid.NewGuid().ToString(), "test_command_string"), routingContext, null);

            routerContext.ParsedCommand.Should().BeNull();
            var result = await commandRouter.RouteCommandAsync(routerContext);
            routerContext.ParsedCommand.Should().BeNull();

            commandParser.Called.Should().BeTrue();
            commandHandler.Called.Should().BeTrue();
        }

        [Fact]
        public async Task Router_Calls_Parser_And_Handler()
        {
            commandParser.Called.Should().BeFalse();
            commandHandler.Called.Should().BeFalse();

            CommandContext routerContext = new(new(Guid.NewGuid().ToString(), "test_command_string"), routingContext, null);
            await commandRouter.RouteCommandAsync(routerContext); ;

            commandParser.Called.Should().BeTrue();
            commandParser.PassedContext.Should().BeSameAs(routerContext);

            commandHandler.Called.Should().BeTrue();
            commandHandler.PassedContext.Should().BeSameAs(routerContext);
        }

        [Fact]
        public async Task Router_Passed_License_To_Handler()
        {
            licenseExtractor.NoLicense = false;

            CommandContext routerContext = new(new(Guid.NewGuid().ToString(), "test_command_string"), routingContext, null);

            routerContext.License.Should().BeNull();
            await commandRouter.RouteCommandAsync(routerContext);
            routerContext.License.Should().NotBeNull();

            licenseExtractor.TestLicense.Should().BeSameAs(commandHandler.PassedContext!.License);

            commandHandler.PassedContext.Should().NotBeNull();
            commandHandler.PassedContext.Should().BeSameAs(routerContext);

            commandParser.Called.Should().BeTrue();
            commandHandler.Called.Should().BeTrue();
        }

        [Fact]
        public async Task Router_Passes_Updated_Context_From_Parser_To_Handler()
        {
            commandHandler.PassedContext.Should().BeNull();

            CommandContext routerContext = new(new(Guid.NewGuid().ToString(), "test_command_string"), routingContext, null);
            routerContext.ParsedCommand.Should().BeNull();
            await commandRouter.RouteCommandAsync(routerContext);

            // These values coming from mock
            commandHandler.PassedContext.Should().NotBeNull();
            commandHandler.PassedContext!.ParsedCommand.Should().NotBeNull();
            commandHandler.PassedContext.ParsedCommand!.Command.Descriptor.Id.Should().Be("test_id");
            commandHandler.PassedContext.ParsedCommand.Command.Descriptor.Name.Should().Be("test_name");
        }

        [Fact]
        public async Task RouterShouldErrorOnNoLicense()
        {
            licenseExtractor.NoLicense = true;

            CommandContext routerContext = new(new(Guid.NewGuid().ToString(), "test_command_string"), routingContext, null);
            Func<Task> func = async () => await commandRouter.RouteCommandAsync(routerContext);
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode("invalid_license").WithErrorDescription("Failed to extract a valid license. Please configure the hosted service correctly.");

            commandParser.Called.Should().BeFalse();
            commandHandler.Called.Should().BeFalse();
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

            CommandContext routerContext = new(new(Guid.NewGuid().ToString(), "test_command_string"), routingContext, null);
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

            commandParser.SetExplicitError = true;

            try
            {
                CommandContext routerContext = new(new(Guid.NewGuid().ToString(), "test_command_string"), routingContext, null);
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
            CommandContext routerContext = new(new(Guid.NewGuid().ToString(), "test_command_string"), routingContext, null);
            await commandRouter.RouteCommandAsync(routerContext);

            eventHandler.BeforeRouteCalled.Should().BeFalse();
            eventHandler.AfterRouteCalled.Should().BeFalse();

            eventHandler.PassedRoute.Should().BeNull();
            eventHandler.PassedCommand.Should().BeNull();
            eventHandler.PassedRouterResult.Should().BeNull();
        }

        private MockCommandHandlerInner commandHandler = null!;
        private MockCommandParserInner commandParser = null!;
        private CommandRouter commandRouter = null!;
        private CancellationTokenSource commandTokenSource = null!;
        private MockTerminalEventHandler eventHandler = null!;
        private IHost host = null!;
        private MockLicenseExtractorInner licenseExtractor = null!;
        private ILogger<CommandRouter> logger = null!;
        private MockTerminalRouterContext routingContext = null!;
        private TerminalOptions terminalOptions = null!;
        private CancellationTokenSource terminalTokenSource = null!;
    }
}
