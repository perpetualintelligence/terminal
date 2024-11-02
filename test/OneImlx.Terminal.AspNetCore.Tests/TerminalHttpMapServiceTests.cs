/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using OneImlx.Terminal.Runtime;
using OneImlx.Test.FluentAssertions;
using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace OneImlx.Terminal.AspNetCore
{
    public class TerminalHttpMapServiceTests
    {
        public TerminalHttpMapServiceTests()
        {
            // Initialize the mocks for the TerminalRouter, Processor, and Logger
            mockTerminalRouter = new Mock<ITerminalRouter<TerminalHttpRouterContext>>();
            mockLogger = new Mock<ILogger<TerminalHttpMapService>>();
            mockProcessor = new Mock<ITerminalProcessor>();
            terminalTokenSource = new CancellationTokenSource();
            commandTokenSource = new CancellationTokenSource();
            mockStartContext = new TerminalStartContext(TerminalStartMode.Http, terminalTokenSource.Token, commandTokenSource.Token, null);

            // Create an instance of TerminalHttpMapService with the mocked dependencies
            terminalHttpMapService = new TerminalHttpMapService(mockTerminalRouter.Object, mockProcessor.Object, mockLogger.Object);
        }

        // Test case to validate that the command is processed successfully and enqueued in the queue
        [Fact]
        public async Task RouteCommand_Adds_Command_Successfully()
        {
            // Arrange
            var request = new TerminalJsonCommandRequest("test-command");
            var context = new DefaultHttpContext();

            // Create a MemoryStream to simulate the HTTP request body with the serialized command
            var stream = new MemoryStream();
            await JsonSerializer.SerializeAsync(stream, request);
            stream.Position = 0;
            context.Request.Body = stream;

            // Mock the router and processor behavior
            mockTerminalRouter.Setup(x => x.IsRunning).Returns(true);
            mockProcessor.Setup(x => x.IsProcessing).Returns(true);

            // Setup processor add method to capture the added item
            TerminalProcessorRequest? addedRequest = null;
            mockProcessor.Setup(x => x.AddAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string, string>((message, endpoint, senderId) =>
                {
                    addedRequest = new TerminalProcessorRequest("id1", message, endpoint, senderId);
                });

            // Act
            addedRequest.Should().BeNull();
            await terminalHttpMapService.RouteCommandAsync(context);

            // Assert
            addedRequest.Should().NotBeNull();
            addedRequest!.Id.Should().Be("id1");
            addedRequest.CommandString.Should().Be("test-command");
            addedRequest.SenderEndpoint.Should().Be("$unknown$");
            addedRequest.SenderId.Should().NotBeEmpty();
        }

        // Test case to validate that a missing command string results in an exception
        [Fact]
        public async Task RouteCommand_Throws_When_Command_Is_Missing()
        {
            // Arrange
            var request = new TerminalJsonCommandRequest("  "); // Empty command string
            var context = new DefaultHttpContext();

            // Create a MemoryStream to simulate the HTTP request body with the serialized command
            var stream = new MemoryStream();
            await JsonSerializer.SerializeAsync(stream, request);
            stream.Position = 0;
            context.Request.Body = stream;

            // Act
            Func<Task> act = async () => await terminalHttpMapService.RouteCommandAsync(context);

            // Assert
            await act.Should().ThrowAsync<TerminalException>()
                .WithErrorCode("missing_command")
                .WithErrorDescription("The command is missing in the HTTP request.");
        }

        // Test case to validate that if the processor is not processing, the system throws an exception
        [Fact]
        public async Task RouteCommand_Throws_When_Processor_Is_Not_Processing()
        {
            // Arrange
            var request = new TerminalJsonCommandRequest("test-command");
            var context = new DefaultHttpContext();

            // Create a MemoryStream to simulate the HTTP request body with the serialized command
            var stream = new MemoryStream();
            await JsonSerializer.SerializeAsync(stream, request);
            stream.Position = 0;
            context.Request.Body = stream;

            // Mock the router to be running but the processor not processing
            mockTerminalRouter.Setup(x => x.IsRunning).Returns(true);
            mockProcessor.Setup(x => x.IsProcessing).Returns(false);

            // Act
            Func<Task> act = async () => await terminalHttpMapService.RouteCommandAsync(context);

            // Assert
            await act.Should().ThrowAsync<TerminalException>()
                .WithErrorCode("server_error")
                .WithErrorDescription("The terminal processor is not processing.");
        }

        // Test case to validate that if the router is not running, the system throws an exception
        [Fact]
        public async Task RouteCommand_Throws_When_Router_Is_Not_Running()
        {
            // Arrange
            var request = new TerminalJsonCommandRequest("test-command");
            var context = new DefaultHttpContext();

            // Create a MemoryStream to simulate the HTTP request body with the serialized command
            var stream = new MemoryStream();
            await JsonSerializer.SerializeAsync(stream, request);
            stream.Position = 0;
            context.Request.Body = stream;

            // Mock the router to be not running
            mockTerminalRouter.Setup(x => x.IsRunning).Returns(false);

            // Act
            Func<Task> act = async () => await terminalHttpMapService.RouteCommandAsync(context);

            // Assert
            await act.Should().ThrowAsync<TerminalException>()
                .WithErrorCode("server_error")
                .WithErrorDescription("The terminal HTTP router is not running.");
        }

        private readonly CancellationTokenSource commandTokenSource;
        private readonly Mock<ILogger<TerminalHttpMapService>> mockLogger;
        private readonly Mock<ITerminalProcessor> mockProcessor;
        private readonly TerminalStartContext mockStartContext;
        private readonly Mock<ITerminalRouter<TerminalHttpRouterContext>> mockTerminalRouter;
        private readonly TerminalHttpMapService terminalHttpMapService;
        private readonly CancellationTokenSource terminalTokenSource;
    }
}
