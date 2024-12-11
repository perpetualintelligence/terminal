/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using FluentAssertions;
using Grpc.Core;
using Moq;
using OneImlx.Terminal.Configuration.Options;
using OneImlx.Terminal.Runtime;
using OneImlx.Test.FluentAssertions;
using Xunit;
using OneImlx.Terminal.Commands;

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

        [Fact]
        public async Task RouteCommand_Processes_Command_Successfully()
        {
            // Real command queue used for testing the behavior of queuing items
            var mockCommandQueue = new TerminalProcessor(
                Mock.Of<ICommandRouter>(), Mock.Of<ITerminalExceptionHandler>(),
                Microsoft.Extensions.Options.Options.Create(new TerminalOptions()),
                new TerminalAsciiTextHandler(),
                Mock.Of<ILogger<TerminalProcessor>>());

            // Ensure the terminal router is running
            mockTerminalRouter.Setup(x => x.IsRunning).Returns(true);
            mockProcessor.Setup(x => x.IsProcessing).Returns(true);

            TerminalInput terminalInput = TerminalInput.Single("id1", "test-command");
            TerminalOutput? addedOutput = null;
            mockProcessor.Setup(x => x.ExecuteAsync(It.IsAny<TerminalInput>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback<TerminalInput, string?, string?>((input, senderId, senderEndpoint) =>
                {
                    // Create and assign a mock response based on the input parameters
                    addedOutput = new TerminalOutput(terminalInput, ["any"], senderId, senderEndpoint);
                })
                .ReturnsAsync(() => addedOutput!);

            // Act
            addedOutput.Should().BeNull();
            var input = new TerminalGrpcRouterProtoInput { InputJson = JsonSerializer.Serialize(terminalInput) };
            var response = await terminalGrpcMapService.RouteCommand(input, testServerCallContext);
            response.OutputJson.Should().Be("{\"input\":{\"batch_id\":null,\"requests\":[{\"id\":\"id1\",\"raw\":\"test-command\"}]},\"results\":[\"any\"],\"sender_endpoint\":null,\"sender_id\":null}");

            // Assert
            addedOutput.Should().NotBeNull();
            addedOutput!.Input.Requests.Should().HaveCount(1);

            addedOutput.Input.Requests[0].Id.Should().Be("id1");
            addedOutput.Input.Requests[0].Raw.Should().Be("test-command");
            addedOutput.Input.BatchId.Should().BeNull();

            addedOutput.Results.Should().HaveCount(1);
            addedOutput.Results[0].Should().Be("any");
        }

        // Test case to validate that if the CommandQueue is null, the system throws an exception
        [Fact]
        public async Task RouteCommand_Throws_When_Processor_Is_Not_Running()
        {
            // Arrange
            var input = new TerminalGrpcRouterProtoInput { InputJson = "test-command" };
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
            var input = new TerminalGrpcRouterProtoInput { InputJson = "test-command" };
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
