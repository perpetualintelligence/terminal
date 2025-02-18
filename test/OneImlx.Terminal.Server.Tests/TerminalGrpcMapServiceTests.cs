/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using FluentAssertions;
using Grpc.Core;
using Moq;
using OneImlx.Terminal.Commands;
using OneImlx.Terminal.Configuration.Options;
using OneImlx.Terminal.Runtime;
using OneImlx.Test.FluentAssertions;
using Xunit;

namespace OneImlx.Terminal.Server
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
                new TerminalTextHandler(StringComparison.OrdinalIgnoreCase, Encoding.ASCII),
                Mock.Of<ILogger<TerminalProcessor>>());

            // Ensure the terminal router is running
            mockTerminalRouter.Setup(x => x.IsRunning).Returns(true);
            mockProcessor.Setup(x => x.IsProcessing).Returns(true);

            TerminalInputOutput terminalInput = TerminalInputOutput.Single("id1", "test-command");
            mockProcessor.Setup(x => x.ExecuteAsync(It.IsAny<TerminalInputOutput>()));

            var input = new TerminalGrpcRouterProtoInput { InputJson = JsonSerializer.Serialize(terminalInput) };
            var response = await terminalGrpcMapService.RouteCommand(input, testServerCallContext);
            response.OutputJson.Should().Be("{\"batch_id\":null,\"requests\":[{\"id\":\"id1\",\"is_error\":false,\"raw\":\"test-command\",\"result\":null}],\"sender_endpoint\":null,\"sender_id\":null}");
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
