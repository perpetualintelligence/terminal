/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using OneImlx.Terminal.Commands.Routers;
using OneImlx.Terminal.Configuration.Options;
using OneImlx.Terminal.Runtime;
using OneImlx.Test.FluentAssertions;
using System;
using System.Threading;
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
            mockProcessor = new Mock<ITerminalProcessor>();
            terminalTokenSource = new CancellationTokenSource();
            commandTokenSource = new CancellationTokenSource();

            // Create an instance of TerminalGrpcMapService with the mocked dependencies
            terminalGrpcMapService = new TerminalGrpcMapService(mockTerminalRouter.Object, mockProcessor.Object, mockLogger.Object);

            // Create a TestServerCallContext to simulate gRPC context with a "test_peer"
            testServerCallContext = new MockServerCallContext("test_peer");
        }

        // Test case to validate that the command is processed successfully and enqueued in the queue
        [Fact]
        public async Task RouteCommand_Processes_Command_Successfully()
        {
            // Arrange
            var input = new TerminalGrpcRouterProtoInput { Request = "test-command" };

            // Real command queue used for testing the behavior of queuing items
            var mockCommandQueue = new TerminalProcessor(
                Mock.Of<ICommandRouter>(), Mock.Of<ITerminalExceptionHandler>(),
                Options.Create(new TerminalOptions()),
                new TerminalAsciiTextHandler(),
                Mock.Of<ILogger<TerminalProcessor>>());

            // Ensure the terminal router is running
            mockTerminalRouter.Setup(x => x.IsRunning).Returns(true);
            mockProcessor.Setup(x => x.IsProcessing).Returns(true);
            mockProcessor.Setup(x => x.SerializeToJsonString(It.IsAny<object?[]>())).Returns("serialized_response");

            TerminalResponse? addedResponse = null;
            mockProcessor.Setup(x => x.ProcessRequestAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string?, string?>((raw, senderId, senderEndpoint) =>
                {
                    // Create and assign a mock response based on the input parameters
                    addedResponse = new TerminalResponse(1, null, senderId, senderEndpoint);
                    addedResponse.Commands[0] = new TerminalRequest("id1", raw);
                })
                .ReturnsAsync(() => addedResponse!);

            // Act
            addedResponse.Should().BeNull();
            var response = await terminalGrpcMapService.RouteCommand(input, testServerCallContext);
            response.Response.Should().Be("serialized_response");

            // Assert
            addedResponse.Should().NotBeNull();
            addedResponse!.Commands.Should().HaveCount(1);

            addedResponse.Commands[0].Id.Should().Be("id1");
            addedResponse.Commands[0].Raw.Should().Be("test-command");
            addedResponse.BatchId.Should().BeNull();
        }

        // Test case to validate that a missing command string results in an exception
        [Fact]
        public async Task RouteCommand_Throws_When_Command_Is_Missing()
        {
            // Arrange
            var input = new TerminalGrpcRouterProtoInput { Request = "  " }; // Empty command string

            // Setup the real command queue
            var mockCommandQueue = new TerminalProcessor(
                Mock.Of<ICommandRouter>(), Mock.Of<ITerminalExceptionHandler>(),
                Options.Create(new TerminalOptions()),
                new TerminalAsciiTextHandler(),
                Mock.Of<ILogger<TerminalProcessor>>());

            // Act
            Func<Task> act = async () => await terminalGrpcMapService.RouteCommand(input, testServerCallContext);

            // Assert
            await act.Should().ThrowAsync<TerminalException>()
                .WithErrorCode("missing_command")
                .WithErrorDescription("The command is missing in the gRPC request.");
        }

        // Test case to validate that if the CommandQueue is null, the system throws an exception
        [Fact]
        public async Task RouteCommand_Throws_When_Processor_Is_Not_Running()
        {
            // Arrange
            var input = new TerminalGrpcRouterProtoInput { Request = "test-command" };
            mockTerminalRouter.Setup(x => x.IsRunning).Returns(true);
            mockProcessor.Setup(x => x.IsProcessing).Returns(false);

            // Act
            Func<Task> act = async () => await terminalGrpcMapService.RouteCommand(input, testServerCallContext);

            // Assert
            await act.Should().ThrowAsync<TerminalException>()
                .WithErrorCode("server_error")
                .WithErrorDescription("The terminal processor is not processing.");
        }

        // Test case to validate that if the CommandQueue is null, the system throws an exception
        [Fact]
        public async Task RouteCommand_Throws_When_Router_Is_Not_Running()
        {
            // Arrange
            var input = new TerminalGrpcRouterProtoInput { Request = "test-command" };
            mockTerminalRouter.Setup(x => x.IsRunning).Returns(false);
            mockProcessor.Setup(x => x.IsProcessing).Returns(true);

            // Act
            Func<Task> act = async () => await terminalGrpcMapService.RouteCommand(input, testServerCallContext);

            // Assert
            await act.Should().ThrowAsync<TerminalException>()
                .WithErrorCode("server_error")
                .WithErrorDescription("The terminal gRPC router is not running.");
        }

        private readonly CancellationTokenSource commandTokenSource;
        private readonly Mock<ILogger<TerminalGrpcMapService>> mockLogger;
        private readonly Mock<ITerminalProcessor> mockProcessor;
        private readonly Mock<ITerminalRouter<TerminalGrpcRouterContext>> mockTerminalRouter;
        private readonly TerminalGrpcMapService terminalGrpcMapService;
        private readonly CancellationTokenSource terminalTokenSource;
        private readonly ServerCallContext testServerCallContext;
    }
}
