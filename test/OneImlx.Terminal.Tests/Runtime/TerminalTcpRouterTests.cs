/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using FluentAssertions;
using Moq;
using OneImlx.Terminal.Commands.Routers;
using OneImlx.Terminal.Configuration.Options;
using OneImlx.Terminal.Mocks;
using OneImlx.Test.FluentAssertions;
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
            textHandler = new TerminalAsciiTextHandler();

            // Set up Cancellation Tokens
            terminalTokenSource = new CancellationTokenSource();
            commandTokenSource = new CancellationTokenSource();

            // Logger and options
            mockListLoggerFactory = new MockListLoggerFactory();
            loggerMock = mockListLoggerFactory.CreateLogger<TerminalTcpRouter>();
            options = new TerminalOptions();

            // Text encoding setup for mock
            textHandlerMock.Setup(txt => txt.Encoding).Returns(Encoding.UTF8);

            // Start context
            startContext = new TerminalStartContext(TerminalStartMode.Tcp, terminalTokenSource.Token, commandTokenSource.Token, null);

            // Mock the terminal processor id
            terminalProcessorMock.Setup(x => x.NewUniqueId(It.IsAny<string>())).Returns(() => Guid.NewGuid().ToString());
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
            var context = new TerminalTcpRouterContext(routerIpEndpoint, startContext);

            var tcpRouter = CreateTcpRouter();

            // Act
            tcpRouter.IsRunning.Should().BeFalse();
            var routerTask = tcpRouter.RunAsync(context);
            tcpRouter.IsRunning.Should().BeTrue();

            // Send a real TCP message
            await SendTcpMessageAsync("test message", routerIpEndpoint);
            await Task.Delay(200); // Allow time for processing
            terminalTokenSource.Cancel(); // Stop the router
            await routerTask;

            tcpRouter.IsRunning.Should().BeFalse();

            // Verify invocations
            terminalProcessorMock.Verify(x => x.StartProcessing(context), Times.Once);
            terminalProcessorMock.Verify(x => x.AddRequestAsync(
                "test message",
                It.Is<string>(ctx => ctx.StartsWith("127.0.0.1")),
                It.IsAny<string>()), Times.Once);
            terminalProcessorMock.Verify(x => x.StopProcessingAsync(options.Router.Timeout), Times.Once);
        }

        [Fact]
        public async Task RunAsync_Handles_Cancellation_And_ReceiveFailure()
        {
            // Arrange
            var routerPort = FreeTcpPort();
            var routerIpEndpoint = new IPEndPoint(IPAddress.Loopback, routerPort);
            var context = new TerminalTcpRouterContext(routerIpEndpoint, startContext);

            // Cancel router task
            terminalTokenSource.CancelAfter(1200);

            // Act
            var tcpRouter = CreateTcpRouter();
            var routerTask = tcpRouter.RunAsync(context);
            await Task.Delay(1000); // Let it run for a bit
            await routerTask;

            // Logs
            mockListLoggerFactory.AllLogMessages.Should().HaveCount(3);
            mockListLoggerFactory.AllLogMessages[0].Should().Be($"Terminal TCP router started. endpoint={context.IPEndPoint}");
            mockListLoggerFactory.AllLogMessages[1].Should().Be("Terminal TCP router canceled.");
            mockListLoggerFactory.AllLogMessages[2].Should().Be($"Terminal TCP router stopped. endpoint={context.IPEndPoint}");

            // Assert that the exception is passed to the exception handler with correct details
            exceptionHandlerMock.Verify(x => x.HandleExceptionAsync(It.Is<TerminalExceptionHandlerContext>(ctx =>
                ctx.Exception.GetType().Equals(typeof(OperationCanceledException)))),
                Times.Once);
        }

        [Fact]
        public async Task RunAsync_Handles_Exception_During_Processing()
        {
            // Arrange
            var routerPort = FreeTcpPort();
            var routerIpEndpoint = new IPEndPoint(IPAddress.Loopback, routerPort);
            var context = new TerminalTcpRouterContext(routerIpEndpoint, startContext);
            var testException = new Exception("Test exception");

            // Setup terminal processor to throw an exception during StartProcessing
            terminalProcessorMock.Setup(x => x.StartProcessing(context)).Throws(testException);

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
            var context = new TerminalTcpRouterContext(routerIpEndpoint, startContext);

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
            var context = new TerminalTcpRouterContext(routerIpEndpoint, startContext);

            var tcpRouter = CreateTcpRouter();

            // Start the router and stop after 3 seconds
            var routerTask = tcpRouter.RunAsync(context);
            await Task.Delay(500);

            // Start 5 clients concurrently
            var clientTasks = new List<Task>();
            for (int i = 0; i < 5; i++)
            {
                clientTasks.Add(SendTcpMessageAsync($"test message {i}", routerIpEndpoint));
            }

            // Wait till client send all messages and wait for 1 second for server to process them
            await Task.WhenAll(clientTasks);
            await Task.Delay(1000);

            // Stop the router
            terminalTokenSource.Cancel();
            await routerTask;

            // Assert: Verify terminal processor received 5 messages
            for (int i = 0; i < 5; i++)
            {
                terminalProcessorMock.Verify(x => x.AddRequestAsync(
                $"test message {i}",
                It.Is<string>(ctx => ctx.StartsWith("127.0.0.1")),
                It.IsAny<string>()), Times.Once);
            }
        }

        [Fact]
        public async Task RunAsync_Processes_Multiple_Clients_Delimited_Messages_Concurrently()
        {
            // Arrange
            var routerPort = FreeTcpPort();
            var routerIpEndpoint = new IPEndPoint(IPAddress.Loopback, routerPort);
            var context = new TerminalTcpRouterContext(routerIpEndpoint, startContext);

            var tcpRouter = CreateTcpRouter();

            // Start the router and stop after 3 seconds
            var routerTask = tcpRouter.RunAsync(context);
            await Task.Delay(500);

            // Start 5 clients concurrently
            var clientTasks = new List<Task>();
            for (int idx = 0; idx < 5; idx++)
            {
                List<string> messages = [];
                for (int jdx = 0; jdx < 5; jdx++) // Send 5 messages per client
                {
                    var lMsg = $"test message {idx}.{jdx}";
                    messages.Add(lMsg);
                }

                var delimitedMessage = TerminalServices.CreateBatch(options, [.. messages]);
                clientTasks.Add(Task.Run(async () =>
                {
                    await SendTcpMessageAsync(delimitedMessage, routerIpEndpoint);
                }));
            }

            // Wait till client send all messages and wait for 1 second for server to process them
            await Task.WhenAll(clientTasks);
            await Task.Delay(1000);

            // Stop the router
            terminalTokenSource.Cancel();
            await routerTask;

            // Assert: Verify terminal processor received 5 messages
            for (int idx = 0; idx < 5; idx++)
            {
                List<string> messages = [];
                for (int jdx = 0; jdx < 5; jdx++) // Send 5 messages per client
                {
                    var lMsg = $"test message {idx}.{jdx}";
                    messages.Add(lMsg);
                }

                var delimitedMessage = TerminalServices.CreateBatch(options, [.. messages]);
                terminalProcessorMock.Verify(x => x.AddRequestAsync(
                delimitedMessage,
                It.Is<string>(ctx => ctx.Contains("127.0.0.1")),
                It.IsAny<string>()), Times.Once);
            }
        }

        [Fact]
        public async Task RunAsync_Throws_Exception_When_IPEndPoint_Is_Null()
        {
            // Arrange
            var context = new TerminalTcpRouterContext(null!, startContext);
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
            startContext = new TerminalStartContext(TerminalStartMode.Grpc, terminalTokenSource.Token, commandTokenSource.Token, null);
            var routerPort = FreeTcpPort();
            var routerIpEndpoint = new IPEndPoint(IPAddress.Loopback, routerPort);
            var context = new TerminalTcpRouterContext(routerIpEndpoint, startContext);
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
                Options.Create<TerminalOptions>(options),
                commandRouterMock.Object,
                textHandlerMock.Object,
                exceptionHandlerMock.Object,
                terminalProcessorMock.Object,
                loggerMock);
        }

        // Helper to send TCP message
        private async Task SendTcpMessageAsync(string message, IPEndPoint endpoint)
        {
            using (var tcpClient = new TcpClient())
            {
                await tcpClient.ConnectAsync(endpoint.Address, endpoint.Port);
                var stream = tcpClient.GetStream();

                await stream.WriteAsync(textHandler.Encoding.GetBytes(message));
                await stream.FlushAsync(); // Ensures all data is sent
            }
        }

        private readonly Mock<ICommandRouter> commandRouterMock;
        private readonly CancellationTokenSource commandTokenSource;
        private readonly Mock<ITerminalExceptionHandler> exceptionHandlerMock;
        private readonly ILogger<TerminalTcpRouter> loggerMock;
        private readonly MockListLoggerFactory mockListLoggerFactory;
        private readonly TerminalOptions options;
        private readonly Mock<ITerminalProcessor> terminalProcessorMock;
        private readonly CancellationTokenSource terminalTokenSource;
        private readonly TerminalAsciiTextHandler textHandler;
        private readonly Mock<ITerminalTextHandler> textHandlerMock;
        private TerminalStartContext startContext;
    }
}
