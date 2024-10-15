/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Moq;
using OneImlx.Terminal.AspNetCore;
using OneImlx.Terminal.Commands.Routers;
using OneImlx.Terminal.Configuration.Options;
using OneImlx.Terminal.Runtime;
using OneImlx.Test.FluentAssertions;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace OneImlx.Terminal.AspNetCore
{
    public class TerminalGrpcMapServiceTests
    {
        public TerminalGrpcMapServiceTests()
        {
            // Initialize the mocks for the TerminalRouter and Logger
            mockTerminalRouter = new Mock<ITerminalRouter<TerminalGrpcRouterContext>>();
            mockLogger = new Mock<ILogger<TerminalGrpcMapService>>();

            // Create an instance of TerminalGrpcMapService with the mocked dependencies
            terminalGrpcMapService = new TerminalGrpcMapService(mockTerminalRouter.Object, mockLogger.Object);

            // Create a TestServerCallContext to simulate gRPC context with a "test_peer"
            testServerCallContext = new MockServerCallContext("test_peer");
        }

        // Test case to validate that the command is processed successfully and enqueued in the queue
        [Fact]
        public async Task RouteCommand_Should_Enqueue_Command_Successfully()
        {
            // Arrange
            var input = new TerminalGrpcRouterProtoInput { CommandString = "test-command" };

            // Real command queue used for testing the behavior of enqueuing items
            var mockCommandQueue = new TerminalRemoteMessageQueue(
                Mock.Of<ICommandRouter>(), Mock.Of<ITerminalExceptionHandler>(),
                new TerminalOptions(),
                new TerminalGrpcRouterContext(new TerminalStartContext(TerminalStartMode.Grpc, default, default)),
                Mock.Of<ILogger>());

            // Setup the terminal router mock to return the real queue
            mockTerminalRouter.SetupGet(r => r.CommandQueue).Returns(mockCommandQueue);

            // Act
            mockCommandQueue.Count.Should().Be(0); // Ensure the queue is initially empty
            var response = await terminalGrpcMapService.RouteCommand(input, testServerCallContext);

            // Assert
            mockCommandQueue.Count.Should().Be(1); // Queue should have exactly one item
            TerminalRemoteMessageItem[]? items = JsonSerializer.Deserialize<TerminalRemoteMessageItem[]>(response.MessageItemsJson);
            items!.Should().HaveCount(1); // Ensure the response contains one item

            // Validate the properties of the enqueued item
            TerminalRemoteMessageItem item = items.First();
            item.CommandString.Should().Be("test-command");
            item.SenderEndpoint.Should().Be("test_peer");
            item.SenderId.Should().NotBeEmpty(); // SenderId should be generated
        }

        // Test case to validate that a missing command string results in an exception
        [Fact]
        public async Task RouteCommand_Throws_When_Command_Is_Missing()
        {
            // Arrange
            var input = new TerminalGrpcRouterProtoInput { CommandString = "  " }; // Empty command string

            // Setup the real command queue
            var mockCommandQueue = new TerminalRemoteMessageQueue(
                Mock.Of<ICommandRouter>(), Mock.Of<ITerminalExceptionHandler>(),
                new TerminalOptions(),
                new TerminalGrpcRouterContext(new TerminalStartContext(TerminalStartMode.Http, default, default)),
                Mock.Of<ILogger>());

            mockTerminalRouter.SetupGet(r => r.CommandQueue).Returns(mockCommandQueue);

            // Act
            Func<Task> act = async () => await terminalGrpcMapService.RouteCommand(input, testServerCallContext);

            // Assert
            await act.Should().ThrowAsync<TerminalException>()
                .WithErrorCode("missing_command")
                .WithErrorDescription("The command is missing in the gRPC request.");
        }

        // Test case to validate that if the CommandQueue is null, the system throws an exception
        [Fact]
        public async Task RouteCommand_Throws_When_CommandQueue_Is_Null()
        {
            // Arrange
            var input = new TerminalGrpcRouterProtoInput { CommandString = "test-command" };

            // Simulate terminal router not running by returning null for CommandQueue
            mockTerminalRouter.SetupGet(r => r.CommandQueue).Returns((TerminalRemoteMessageQueue?)null);

            // Act
            Func<Task> act = async () => await terminalGrpcMapService.RouteCommand(input, testServerCallContext);

            // Assert
            await act.Should().ThrowAsync<TerminalException>()
                .WithErrorCode("server_error")
                .WithErrorDescription("The terminal gRPC router is not running.");
        }

        // Mock objects used in the test
        private readonly Mock<ILogger<TerminalGrpcMapService>> mockLogger;
        private readonly Mock<ITerminalRouter<TerminalGrpcRouterContext>> mockTerminalRouter;
        private readonly TerminalGrpcMapService terminalGrpcMapService;
        private readonly ServerCallContext testServerCallContext; // Using TestServerCallContext for simulating gRPC context
    }
}
