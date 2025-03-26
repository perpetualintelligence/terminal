/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using OneImlx.Terminal.Commands;
using OneImlx.Terminal.Configuration.Options;
using OneImlx.Terminal.Mocks;
using OneImlx.Terminal.Shared;
using OneImlx.Test.FluentAssertions;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace OneImlx.Terminal.Runtime.Tests
{
    public class TerminalTcpRouterTests
    {
        public TerminalTcpRouterTests()
        {
            // Mock dependencies
            commandRouterMock = new Mock<ICommandRouter>();
            exceptionHandlerMock = new Mock<ITerminalExceptionHandler>();
            terminalProcessorMock = new Mock<ITerminalProcessor>();
            textHandlerMock = new Mock<ITerminalTextHandler>();
            textHandler = new TerminalTextHandler(StringComparison.OrdinalIgnoreCase, Encoding.ASCII);

            // Set up Cancellation Tokens
            terminalTokenSource = new CancellationTokenSource();
            commandTokenSource = new CancellationTokenSource();

            // Logger and options
            mockListLoggerFactory = new MockListLoggerFactory();
            loggerMock = mockListLoggerFactory.CreateLogger<TerminalTcpRouter>();
            options = new TerminalOptions();

            // Text encoding setup for mock
            textHandlerMock.Setup(static txt => txt.Encoding).Returns(Encoding.UTF8);
        }

        public static int FreeTcpPort()
        {
            TcpListener l = new(IPAddress.Loopback, 0);
            l.Start();
            int port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            return port;
        }

        [Fact]
        public async Task RunAsync_Calls_Start_Add_Stop_Processor_Correctly()
        {
            // Arrange
            var routerPort = FreeTcpPort();
            var routerIpEndpoint = new IPEndPoint(IPAddress.Loopback, routerPort);
            var context = new TerminalTcpRouterContext(routerIpEndpoint, TerminalStartMode.Tcp, terminalTokenSource.Token, commandTokenSource.Token, null);

            var tcpRouter = CreateTcpRouter();

            // Act
            tcpRouter.IsRunning.Should().BeFalse();
            var routerTask = tcpRouter.RunAsync(context);
            tcpRouter.IsRunning.Should().BeTrue();

            // Send a real TCP message
            var sentBytes = await SendTcpMessageAsync(TerminalInputOutput.Single("id1", "test message"), routerIpEndpoint);
            await Task.Delay(200); // Allow time for processing
            terminalTokenSource.Cancel(); // Stop the router
            await routerTask;

            tcpRouter.IsRunning.Should().BeFalse();

            // Verify invocations
            terminalProcessorMock.Verify(x => x.StartProcessing(context, true, It.IsAny<Func<TerminalInputOutput, Task>>()), Times.Once);
            terminalProcessorMock.Verify(x => x.StreamAsync(
                sentBytes.Item1,
                sentBytes.Item2,
                It.IsAny<string>(),
                It.Is<string>(ctx => ctx.StartsWith("127.0.0.1"))), Times.Once);
            terminalProcessorMock.Verify(x => x.StopProcessingAsync(5000), Times.Once);
        }

        [Fact]
        public async Task RunAsync_Handles_Cancellation_And_ReceiveFailure()
        {
            // Arrange
            var routerPort = FreeTcpPort();
            var routerIpEndpoint = new IPEndPoint(IPAddress.Loopback, routerPort);
            var context = new TerminalTcpRouterContext(routerIpEndpoint, TerminalStartMode.Tcp, terminalTokenSource.Token, commandTokenSource.Token, null);

            // Cancel router task
            terminalTokenSource.CancelAfter(2000);

            // Act
            var tcpRouter = CreateTcpRouter();
            var routerTask = tcpRouter.RunAsync(context);
            await Task.Delay(1500); // Let it run for a bit
            await routerTask;

            // Logs
            mockListLoggerFactory.AllLogMessages.Should().HaveCount(3);
            mockListLoggerFactory.AllLogMessages[0].Should().Be($"Terminal TCP router started. endpoint={context.IPEndPoint}");
            mockListLoggerFactory.AllLogMessages[1].Should().Be("Terminal TCP router canceled.");
            mockListLoggerFactory.AllLogMessages[2].Should().Be($"Terminal TCP router stopped. endpoint={context.IPEndPoint}");

            // Assert that the exception is passed to the exception handler with correct details
            exceptionHandlerMock.Verify(static x => x.HandleExceptionAsync(It.Is<TerminalExceptionHandlerContext>(static ctx =>
                ctx.Exception.GetType().Equals(typeof(OperationCanceledException)))),
                Times.Once);
        }

        [Fact]
        public async Task RunAsync_Handles_Exception_During_Processing()
        {
            // Arrange
            var routerPort = FreeTcpPort();
            var routerIpEndpoint = new IPEndPoint(IPAddress.Loopback, routerPort);
            var context = new TerminalTcpRouterContext(routerIpEndpoint, TerminalStartMode.Tcp, terminalTokenSource.Token, commandTokenSource.Token, null);
            var testException = new Exception("Test exception");

            // Setup terminal processor to throw an exception during StartProcessing
            terminalProcessorMock.Setup(x => x.StartProcessing(context, true, It.IsAny<Func<TerminalInputOutput, Task>>())).Throws(testException);

            var tcpRouter = CreateTcpRouter();

            // Act
            await tcpRouter.RunAsync(context);

            // Assert: Exception handler should be called with the correct exception
            exceptionHandlerMock.Verify(x => x.HandleExceptionAsync(It.Is<TerminalExceptionHandlerContext>(ctx =>
                ctx.Exception == testException)), Times.Once);
        }

        [Fact]
        public async Task RunAsync_Logs_Start_And_Stop_Messages()
        {
            // Arrange
            var routerPort = FreeTcpPort();
            var routerIpEndpoint = new IPEndPoint(IPAddress.Loopback, routerPort);
            var context = new TerminalTcpRouterContext(routerIpEndpoint, TerminalStartMode.Tcp, terminalTokenSource.Token, commandTokenSource.Token, null);

            var tcpRouter = CreateTcpRouter();

            // Act
            var routerTask = tcpRouter.RunAsync(context);
            await Task.Delay(1000); // Let the router start
            terminalTokenSource.Cancel(); // Cancel the task to stop it
            await routerTask;

            // Assert: Verify the log messages for start and stop
            mockListLoggerFactory.AllLogMessages.First().Should().Be($"Terminal TCP router started. endpoint={context.IPEndPoint}");
            mockListLoggerFactory.AllLogMessages.Last().Should().Be($"Terminal TCP router stopped. endpoint={context.IPEndPoint}");
        }

        [Fact]
        public async Task RunAsync_Processes_Multiple_Clients_Concurrently()
        {
            // Arrange
            var routerPort = FreeTcpPort();
            var routerIpEndpoint = new IPEndPoint(IPAddress.Loopback, routerPort);
            var context = new TerminalTcpRouterContext(routerIpEndpoint, TerminalStartMode.Tcp, terminalTokenSource.Token, commandTokenSource.Token, null);

            var tcpRouter = CreateTcpRouter();

            // Start the router and stop after a brief delay
            var routerTask = tcpRouter.RunAsync(context);
            await Task.Delay(500);

            // Start 5 clients concurrently
            var clientTasks = new List<Task<(byte[], int)>>();
            for (int idx = 0; idx < 5; idx++)
            {
                int localIdx = idx; // Ensure correct variable capture for each iteration
                Task<(byte[], int)> clientTask = Task.Run(async () =>
                {
                    string message = $"test message {localIdx}";
                    return await SendTcpMessageAsync(TerminalInputOutput.Single($"id{localIdx}", message), routerIpEndpoint);
                });

                clientTasks.Add(clientTask);
            }

            // Wait for all client tasks to complete and give time for server to process.
            (byte[], int)[] sentBytesArray = await Task.WhenAll(clientTasks);
            await Task.Delay(2000);

            // Stop the router
            terminalTokenSource.Cancel();
            await routerTask;

            // Assert: Verify terminal processor received the expected number of messages
            for (int i = 0; i < 5; i++)
            {
                terminalProcessorMock.Verify(x => x.StreamAsync(
                    sentBytesArray[i].Item1,
                    sentBytesArray[i].Item2,
                    It.IsAny<string>(),
                    It.Is<string>(ctx => ctx.StartsWith("127.0.0.1"))), Times.Once);
            }
        }

        [Fact]
        public async Task RunAsync_Processes_Multiple_Clients_Delimited_Messages_Concurrently()
        {
            // Arrange
            var routerPort = FreeTcpPort();
            var routerIpEndpoint = new IPEndPoint(IPAddress.Loopback, routerPort);
            var context = new TerminalTcpRouterContext(routerIpEndpoint, TerminalStartMode.Tcp, terminalTokenSource.Token, commandTokenSource.Token, null);

            var tcpRouter = CreateTcpRouter();

            // Start the router and allow it to initialize
            var routerTask = tcpRouter.RunAsync(context);
            await Task.Delay(500);

            // Start 5 clients concurrently, each sending a batch of delimited messages
            var clientTasks = new List<Task<(byte[], int)>>();
            for (int idx = 0; idx < 5; idx++)
            {
                int localIdx = idx; // Ensure correct variable capture for each iteration
                Task<(byte[], int)> clientTask = Task.Run(async () =>
                {
                    OrderedDictionary messages = [];
                    for (int jdx = 0; jdx < 5; jdx++) // Create 5 messages per client
                    {
                        messages.Add($"id {localIdx}.{jdx}", $"test message {localIdx}.{jdx}");
                    }

                    TerminalInputOutput input = TerminalInputOutput.Batch($"batch{localIdx}", messages.Keys.Cast<string>().ToArray(), messages.Values.Cast<string>().ToArray());
                    return await SendTcpMessageAsync(input, routerIpEndpoint);
                });

                clientTasks.Add(clientTask);
            }

            // Wait for all client tasks to complete and give time for server to process
            (byte[], int)[] sentBytesArray = await Task.WhenAll(clientTasks);
            await Task.Delay(2000);

            // Stop the router
            terminalTokenSource.Cancel();
            await routerTask;

            // Assert: Verify terminal processor received the expected number of delimited messages
            for (int i = 0; i < 5; i++)
            {
                terminalProcessorMock.Verify(x => x.StreamAsync(
                    sentBytesArray[i].Item1,
                    sentBytesArray[i].Item2,
                    It.IsAny<string>(),
                    It.Is<string>(ctx => ctx.StartsWith("127.0.0.1"))), Times.Once);
            }
        }

        [Fact]
        public async Task RunAsync_Throws_Exception_When_IPEndPoint_Is_Null()
        {
            // Arrange
            var context = new TerminalTcpRouterContext(null!, TerminalStartMode.Tcp, terminalTokenSource.Token, commandTokenSource.Token, null);
            var tcpRouter = CreateTcpRouter();

            // Act & Assert
            Func<Task> act = async () => await tcpRouter.RunAsync(context);
            await act.Should().ThrowAsync<TerminalException>()
                .WithErrorCode("invalid_configuration")
                .WithErrorDescription("The network IP endpoint is missing in the TCP server routing request.");
        }

        [Fact]
        public async Task RunAsync_Throws_Exception_When_Start_Mode_Is_Not_Tcp()
        {
            // Arrange
            var routerPort = FreeTcpPort();
            var routerIpEndpoint = new IPEndPoint(IPAddress.Loopback, routerPort);
            var context = new TerminalTcpRouterContext(routerIpEndpoint, TerminalStartMode.Grpc, terminalTokenSource.Token, commandTokenSource.Token, null);
            var tcpRouter = CreateTcpRouter();

            // Act & Assert
            Func<Task> act = async () => await tcpRouter.RunAsync(context);
            await act.Should().ThrowAsync<TerminalException>()
                .WithErrorCode("invalid_configuration")
                .WithErrorDescription("The requested start mode is not valid for TCP routing. start_mode=Grpc");
        }

        // Helper to create a TcpRouter instance
        private TerminalTcpRouter CreateTcpRouter()
        {
            return new TerminalTcpRouter(
                Microsoft.Extensions.Options.Options.Create(options),
                exceptionHandlerMock.Object,
                terminalProcessorMock.Object,
                loggerMock);
        }

        // Helper to send TCP message
        private async Task<(byte[], int)> SendTcpMessageAsync(TerminalInputOutput input, IPEndPoint endpoint)
        {
            // fix this test should not be relying on hardcoaded 4096 buffer size the tcp router passes buffer and
            // length to the terminal processor
            byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(input);
            byte[] buffer = new byte[4096];
            bytes.CopyTo(buffer, 0);
            using (var tcpClient = new TcpClient())
            {
                await tcpClient.ConnectAsync(endpoint.Address, endpoint.Port);
                var stream = tcpClient.GetStream();

                await stream.WriteAsync(bytes);
                await stream.FlushAsync(); // Ensures all data is sent
            }
            return (buffer, bytes.Length);
        }

        private readonly Mock<ICommandRouter> commandRouterMock;
        private readonly CancellationTokenSource commandTokenSource;
        private readonly Mock<ITerminalExceptionHandler> exceptionHandlerMock;
        private readonly ILogger<TerminalTcpRouter> loggerMock;
        private readonly MockListLoggerFactory mockListLoggerFactory;
        private readonly TerminalOptions options;
        private readonly Mock<ITerminalProcessor> terminalProcessorMock;
        private readonly CancellationTokenSource terminalTokenSource;
        private readonly TerminalTextHandler textHandler;
        private readonly Mock<ITerminalTextHandler> textHandlerMock;
    }
}
