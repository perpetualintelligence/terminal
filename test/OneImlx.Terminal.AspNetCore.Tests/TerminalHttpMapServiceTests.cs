/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using OneImlx.Terminal.Commands.Routers;
using OneImlx.Terminal.Configuration.Options;
using OneImlx.Terminal.Runtime;
using OneImlx.Test.FluentAssertions;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace OneImlx.Terminal.AspNetCore
{
    public class TerminalHttpMapServiceTests
    {
        public TerminalHttpMapServiceTests()
        {
            // Initialize the mocks
            mockTerminalRouter = new Mock<ITerminalRouter<TerminalHttpRouterContext>>();
            mockLogger = new Mock<ILogger<TerminalHttpMapService>>();

            // Initialize the TerminalHttpMapService instance with mocked dependencies
            terminalHttpMapService = new TerminalHttpMapService(mockTerminalRouter.Object, mockLogger.Object);

            // Create a DefaultHttpContext to simulate an HTTP request
            httpContext = new DefaultHttpContext();
        }

        [Fact]
        public async Task EnqueueCommandAsync_Should_Process_Command_Successfully()
        {
            // Arrange
            string jsonCommand = JsonSerializer.Serialize(new TerminalJsonCommandRequest("test-command"));
            httpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(jsonCommand));

            var mockCommandQueue = new TerminalRemoteMessageQueue(
                Mock.Of<ICommandRouter>(), Mock.Of<ITerminalExceptionHandler>(),
                new TerminalOptions(),
                new TerminalHttpRouterContext(new IPEndPoint(IPAddress.Loopback, 8000), new TerminalStartContext(TerminalStartMode.Http, default, default)),
                Mock.Of<ILogger>());
            mockTerminalRouter.SetupGet(r => r.CommandQueue).Returns(mockCommandQueue);

            // Act
            mockCommandQueue.Count.Should().Be(0);
            var items = await terminalHttpMapService.RouteCommandAsync(httpContext);

            // Assert
            mockCommandQueue.Count.Should().Be(1);
            items.Should().HaveCount(1);
            TerminalRemoteMessageItem item = items.First();
            item.Id.Should().NotBeEmpty();
            item.CommandString.Should().Be("test-command");
            item.SenderEndpoint.Should().Be("$unknown$");
            item.SenderId.Should().NotBeEmpty();
        }

        [Fact]
        public async Task EnqueueCommandAsync_Throws_When_Command_Is_Missing()
        {
            // Arrange
            string jsonCommand = JsonSerializer.Serialize(new TerminalJsonCommandRequest("")); // Missing command
            httpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(jsonCommand));
            var mockCommandQueue = new TerminalRemoteMessageQueue(
                Mock.Of<ICommandRouter>(), Mock.Of<ITerminalExceptionHandler>(),
                new TerminalOptions(),
                new TerminalHttpRouterContext(new IPEndPoint(IPAddress.Loopback, 8000), new TerminalStartContext(TerminalStartMode.Http, default, default)),
                Mock.Of<ILogger>());

            mockTerminalRouter.SetupGet(r => r.CommandQueue).Returns(mockCommandQueue);

            // Act
            Func<Task> act = async () => await terminalHttpMapService.RouteCommandAsync(httpContext);

            // Assert
            await act.Should().ThrowAsync<TerminalException>()
                .WithErrorCode("missing_command")
                .WithErrorDescription("The command is missing in the HTTP request.");
        }

        [Fact]
        public async Task EnqueueCommandAsync_Throws_When_CommandQueue_Is_Null()
        {
            // Arrange
            string jsonCommand = JsonSerializer.Serialize(new TerminalJsonCommandRequest("test-command"));
            httpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(jsonCommand));

            // Simulate router not running
            mockTerminalRouter.SetupGet(r => r.CommandQueue).Returns((TerminalRemoteMessageQueue?)null);

            // Act
            Func<Task> act = async () => await terminalHttpMapService.RouteCommandAsync(httpContext);

            // Assert
            await act.Should().ThrowAsync<TerminalException>()
                .WithErrorCode("server_error")
                .WithErrorDescription("The terminal HTTP router is not running.");
        }

        private readonly DefaultHttpContext httpContext;
        private readonly Mock<ILogger<TerminalHttpMapService>> mockLogger;
        private readonly Mock<ITerminalRouter<TerminalHttpRouterContext>> mockTerminalRouter;
        private readonly TerminalHttpMapService terminalHttpMapService;
    }
}
