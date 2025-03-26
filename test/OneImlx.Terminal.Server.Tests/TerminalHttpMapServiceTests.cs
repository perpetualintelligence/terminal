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
using OneImlx.Terminal.Shared;
using OneImlx.Test.FluentAssertions;
using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace OneImlx.Terminal.Server
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

            // Create an instance of TerminalHttpMapService with the mocked dependencies
            terminalHttpMapService = new TerminalHttpMapService(mockTerminalRouter.Object, mockProcessor.Object, mockLogger.Object);
        }

        [Fact]
        public async Task RouteCommand_Processes_Command_Successfully()
        {
            // Arrange
            var context = new DefaultHttpContext();
            using (var resposeStream = new MemoryStream())
            {
                context.Response.Body = resposeStream;

                // Create a MemoryStream to simulate the HTTP request body with the serialized command
                using (var requestStream = new MemoryStream())
                {
                    TerminalInputOutput terminalInput = TerminalInputOutput.Single("id1", "test-command");
                    await JsonSerializer.SerializeAsync(requestStream, terminalInput);
                    requestStream.Position = 0;
                    context.Request.Body = requestStream;
                    context.Request.ContentType = "application/json";

                    // Mock the router and processor behavior
                    mockTerminalRouter.Setup(x => x.IsRunning).Returns(true);
                    mockProcessor.Setup(x => x.IsProcessing).Returns(true);
                    mockProcessor.Setup(x => x.ExecuteAsync(It.IsAny<TerminalInputOutput>()));

                    // Act
                    await terminalHttpMapService.RouteAsync(context);
                    context.Response.ContentType.Should().Be("application/json; charset=utf-8");
                    context.Response.Body.Seek(0, SeekOrigin.Begin);
                    using var reader = new StreamReader(context.Response.Body);
                    string jsonResponse = await reader.ReadToEndAsync();
                    jsonResponse.Should().Be("{\"batch_id\":null,\"requests\":[{\"id\":\"id1\",\"is_error\":false,\"raw\":\"test-command\",\"result\":null}],\"sender_endpoint\":null,\"sender_id\":null}");
                }
            }
        }

        // Test case to validate that if the processor is not processing, the system throws an exception
        [Fact]
        public async Task RouteCommand_Throws_When_Processor_Is_Not_Processing()
        {
            // Arrange
            var input = TerminalInputOutput.Single("test-id", "test-command");
            var context = new DefaultHttpContext();

            // Create a MemoryStream to simulate the HTTP request body with the serialized command
            using (var stream = new MemoryStream())
            {
                await JsonSerializer.SerializeAsync(stream, input);
                stream.Position = 0;
                context.Request.Body = stream;

                // Mock the router to be running but the processor not processing
                mockTerminalRouter.Setup(x => x.IsRunning).Returns(true);
                mockProcessor.Setup(x => x.IsProcessing).Returns(false);

                // Act
                Func<Task> act = async () => await terminalHttpMapService.RouteAsync(context);

                // Assert
                await act.Should().ThrowAsync<TerminalException>()
                    .WithErrorCode("server_error")
                    .WithErrorDescription("The terminal processor is not processing.");
            }
        }

        // Test case to validate that if the router is not running, the system throws an exception
        [Fact]
        public async Task RouteCommand_Throws_When_Router_Is_Not_Running()
        {
            // Arrange
            var input = TerminalInputOutput.Single("test-id", "test-command");
            var context = new DefaultHttpContext();

            // Create a MemoryStream to simulate the HTTP request body with the serialized command
            using (var stream = new MemoryStream())
            {
                await JsonSerializer.SerializeAsync(stream, input);
                stream.Position = 0;
                context.Request.Body = stream;

                // Mock the router to be not running
                mockTerminalRouter.Setup(x => x.IsRunning).Returns(false);

                // Act
                Func<Task> act = async () => await terminalHttpMapService.RouteAsync(context);

                // Assert
                await act.Should().ThrowAsync<TerminalException>()
                    .WithErrorCode("server_error")
                    .WithErrorDescription("The terminal HTTP router is not running.");
            }
        }

        private readonly CancellationTokenSource commandTokenSource;
        private readonly Mock<ILogger<TerminalHttpMapService>> mockLogger;
        private readonly Mock<ITerminalProcessor> mockProcessor;
        private readonly Mock<ITerminalRouter<TerminalHttpRouterContext>> mockTerminalRouter;
        private readonly TerminalHttpMapService terminalHttpMapService;
        private readonly CancellationTokenSource terminalTokenSource;
    }
}
