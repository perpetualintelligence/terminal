/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using OneImlx.Terminal.Commands.Routers;
using OneImlx.Terminal.Configuration.Options;
using OneImlx.Terminal.Mocks;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace OneImlx.Terminal.Runtime

{
    public class TerminalUdpRouterTests
    {
        public TerminalUdpRouterTests()
        {
            commandRouterMock = new Mock<ICommandRouter>();
            exceptionHandlerMock = new Mock<ITerminalExceptionHandler>();
            terminalProcessorMock = new Mock<ITerminalProcessor>();
            textHandlerMock = new Mock<ITerminalTextHandler>();
            terminalCancellationSource = new CancellationTokenSource();
            commandCancellationSource = new CancellationTokenSource();
            options = new TerminalOptions();

            mockListLoggerFactory = new();
            loggerMock = mockListLoggerFactory.CreateLogger<TerminalUdpRouter>();

            textHandlerMock.Setup(txt => txt.Encoding).Returns(Encoding.UTF8);
        }

        [Fact]
        public async Task RunAsync_Handles_Cancellation_And_ReceiveFailure()
        {
            // Arrange
            var context = CreateValidContext();
            var udpRouter = CreateUdpRouter();
            terminalProcessorMock.Setup(x => x.StartProcessing(context));

            // Let server sun for sometime
            terminalCancellationSource.CancelAfter(1200);

            // Run router for 1 second
            var routerTask = udpRouter.RunAsync(context);
            await Task.Delay(1000);
            await routerTask;

            // Assert that the exception is passed to the exception handler with correct details
            exceptionHandlerMock.Verify(x => x.HandleExceptionAsync(It.Is<TerminalExceptionHandlerContext>(ctx =>
                ctx.Exception.GetType().Equals(typeof(OperationCanceledException)))),
                Times.Once);
        }

        [Fact]
        public async Task RunAsync_HandlesExceptionDuringProcessing_WithCorrectDetails()
        {
            // Arrange
            var context = CreateValidContext();
            var udpRouter = CreateUdpRouter();
            var testException = new Exception("Test exception");

            // Setup the terminal processor to throw an exception during StartProcessing
            terminalProcessorMock.Setup(x => x.StartProcessing(context)).Throws(testException);

            // Act
            await udpRouter.RunAsync(context);

            // Assert that the exception is passed to the exception handler with correct details
            exceptionHandlerMock.Verify(x => x.HandleExceptionAsync(It.Is<TerminalExceptionHandlerContext>(ctx =>
                ctx.Exception == testException)),
                Times.Once);
        }

        [Fact]
        public async Task RunAsync_Logs_StartAndStop_Messages()
        {
            // Arrange
            var context = CreateValidContext();
            var udpRouter = CreateUdpRouter();

            // Act
            var routerTask = udpRouter.RunAsync(context);
            await SendUdpMessageAsync("test message", context.IPEndPoint);
            await Task.Delay(100);
            terminalCancellationSource.Cancel();
            await routerTask;

            // Assert: Verify log messages
            mockListLoggerFactory.AllLogMessages.Should().HaveCount(2);
            mockListLoggerFactory.AllLogMessages[0].Should().Be($"Terminal UDP router started. endpoint={context.IPEndPoint}");
            mockListLoggerFactory.AllLogMessages[1].Should().Be($"Terminal UDP router stopped. endpoint={context.IPEndPoint}");
        }

        [Fact]
        public async Task RunAsync_StartsAndStops_Correctly()
        {
            var context = CreateValidContext();
            var udpRouter = CreateUdpRouter();

            udpRouter.IsRunning.Should().BeFalse();
            var routerTask = udpRouter.RunAsync(context);
            udpRouter.IsRunning.Should().BeTrue();

            await SendUdpMessageAsync("test message", context.IPEndPoint); // Send a real UDP message
            await Task.Delay(100);
            terminalCancellationSource.Cancel();
            await routerTask;
            udpRouter.IsRunning.Should().BeFalse();

            // Assert
            terminalProcessorMock.Verify(x => x.StartProcessing(context), Times.Once);
            terminalProcessorMock.Verify(x => x.AddRequestAsync("test message", It.Is<string>(ctx => ctx.Contains("127.0.0.1")), It.IsAny<string>()), Times.Once);
            terminalProcessorMock.Verify(x => x.StopProcessingAsync(options.Router.Timeout), Times.Once);
        }

        [Fact]
        public async Task RunAsync_ThrowsException_WhenIPEndPointIsNull()
        {
            // Arrange
            var context = new TerminalUdpRouterContext(null!, new(TerminalStartMode.Udp, terminalCancellationSource.Token, commandCancellationSource.Token, null));
            var udpRouter = CreateUdpRouter();

            // Act & Assert
            Func<Task> act = async () => await udpRouter.RunAsync(context);
            await act.Should().ThrowAsync<TerminalException>()
                .WithMessage("The network IP endpoint is missing in the UDP server routing request.");
        }

        // Helper to create an instance of TerminalUdpRouter
        private TerminalUdpRouter CreateUdpRouter()
        {
            return new TerminalUdpRouter(
                commandRouterMock.Object,
                exceptionHandlerMock.Object,
                Options.Create(options),
                textHandlerMock.Object,
                terminalProcessorMock.Object,
                loggerMock);
        }

        // Helper to create a valid TerminalUdpRouterContext
        private TerminalUdpRouterContext CreateValidContext()
        {
            var endpoint = new IPEndPoint(IPAddress.Loopback, 12345); // Use a real, high-numbered port for the test
            var startContext = new TerminalStartContext(TerminalStartMode.Udp, terminalCancellationSource.Token, commandCancellationSource.Token, null);
            return new TerminalUdpRouterContext(endpoint, startContext);
        }

        // Helper method to send a real UDP message
        private async Task SendUdpMessageAsync(string message, IPEndPoint endpoint)
        {
            using (var client = new UdpClient())
            {
                var messageBytes = Encoding.UTF8.GetBytes(message);
                await client.SendAsync(messageBytes, messageBytes.Length, endpoint);
            }
        }

        private readonly CancellationTokenSource commandCancellationSource;
        private readonly Mock<ICommandRouter> commandRouterMock;
        private readonly Mock<ITerminalExceptionHandler> exceptionHandlerMock;
        private readonly ILogger<TerminalUdpRouter> loggerMock;
        private readonly MockListLoggerFactory mockListLoggerFactory;
        private readonly TerminalOptions options;
        private readonly CancellationTokenSource terminalCancellationSource;
        private readonly Mock<ITerminalProcessor> terminalProcessorMock;
        private readonly Mock<ITerminalTextHandler> textHandlerMock;
    }
}
