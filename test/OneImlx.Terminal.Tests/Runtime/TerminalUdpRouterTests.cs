/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Microsoft.Extensions.Logging;
using OneImlx.Terminal.Configuration.Options;
using OneImlx.Terminal.Mocks;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace OneImlx.Terminal.Runtime
{
    public class TerminalUdpRouterTests
    {
        public TerminalUdpRouterTests()
        {
            commandRouter = new MockCommandRouter();
            exceptionHandler = new MockExceptionPublisher();
            options = MockTerminalOptions.NewAliasOptions();
            textHandler = new TerminalAsciiTextHandler();
            loggerFactory = new MockListLoggerFactory();
            logger = loggerFactory.CreateLogger<TerminalUdpRouter>(); ;

            terminalTokenSource = new();
            commandTokenSource = new();
            startContext = new TerminalStartContext(TerminalStartMode.Udp, terminalTokenSource.Token, commandTokenSource.Token, null, null);
            udpRouter = new TerminalUdpRouter(commandRouter, exceptionHandler, options, textHandler, logger);
        }

        [Fact]
        public async Task Router_Processes_CommandRoute_Exception_Correctly()
        {
            // Throw exception
            commandRouter = new MockCommandRouter(exception: new("Test exception"));
            udpRouter = new TerminalUdpRouter(commandRouter, exceptionHandler, options, textHandler, logger);

            var routerPort = FreeTcpPort();
            var routerIpEndpoint = new IPEndPoint(IPAddress.Loopback, routerPort);

            var context = new TerminalUdpRouterContext(routerIpEndpoint, startContext);

            // Cancel after 2 secs
            terminalTokenSource.CancelAfter(2000);

            Task routerTask = udpRouter.RunAsync(context);
            Task clientTask = Task.Run(async () =>
            {
                await Task.Delay(1000); // Wait for router to start
                await SendUdpMessageAsync("test123", routerIpEndpoint);
            });

            // Wait for both the routing task and the client task to complete
            await Task.WhenAll(routerTask, clientTask);

            commandRouter.RouteCalled.Should().BeTrue();
            commandRouter.MultipleRawString.Count.Should().Be(1);
            commandRouter.MultipleRawString[0].Should().Be("test123");

            exceptionHandler.Called.Should().BeTrue();
            exceptionHandler.MultiplePublishedMessages.Count.Should().Be(1);
            exceptionHandler.MultiplePublishedMessages[0].Should().Be("Test exception");
        }

        [Fact]
        public async Task Router_Processes_CommandRoute_Timeout_Correctly()
        {
            // Throw timeout
            commandRouter = new MockCommandRouter(routeDelay: 5000);
            udpRouter = new TerminalUdpRouter(commandRouter, exceptionHandler, options, textHandler, logger);

            var routerPort = FreeTcpPort();
            var routerIpEndpoint = new IPEndPoint(IPAddress.Loopback, routerPort);

            var context = new TerminalUdpRouterContext(routerIpEndpoint, startContext);

            // Cancel after 2 secs
            terminalTokenSource.CancelAfter(2000);

            Task routerTask = udpRouter.RunAsync(context);
            Task clientTask = Task.Run(async () =>
            {
                await Task.Delay(1000); // Wait for router to start
                await SendUdpMessageAsync("test123", routerIpEndpoint);
            });

            // Wait for both the routing task and the client task to complete
            await Task.WhenAll(routerTask, clientTask);

            commandRouter.RouteCalled.Should().BeTrue();

            exceptionHandler.Called.Should().BeTrue();
            exceptionHandler.MultiplePublishedMessages.Count.Should().Be(1);
            exceptionHandler.MultiplePublishedMessages[0].Should().Be("The command router timed out in 25000 milliseconds.");
        }

        [Fact]
        public async Task Router_Processes_Received_Udp_Message_Correctly()
        {
            var routerPort = FreeTcpPort();
            var routerIpEndpoint = new IPEndPoint(IPAddress.Loopback, routerPort);

            var context = new TerminalUdpRouterContext(routerIpEndpoint, startContext);

            // Cancel after 2 secs
            terminalTokenSource.CancelAfter(2000);

            Task routerTask = udpRouter.RunAsync(context);
            Task clientTask = Task.Run(async () =>
            {
                await Task.Delay(1000); // Wait for router to start
                await SendUdpMessageAsync("test123", routerIpEndpoint);
            });

            // Wait for both the routing task and the client task to complete
            await Task.WhenAll(routerTask, clientTask);

            commandRouter.RouteCalled.Should().BeTrue();
            commandRouter.MultipleRawString.Count.Should().Be(1);
            commandRouter.MultipleRawString[0].Should().Be("test123");

            exceptionHandler.Called.Should().BeFalse();
        }

        [Fact]
        public async Task Router_Processes_Multiple_Received_Udp_Messages_Correctly()
        {
            var routerPort = FreeTcpPort();
            var routerIpEndpoint = new IPEndPoint(IPAddress.Loopback, routerPort);

            var context = new TerminalUdpRouterContext(routerIpEndpoint, startContext);

            // Cancel after 3 seconds to give enough time for messages to be processed
            terminalTokenSource.CancelAfter(3000);

            Task routerTask = udpRouter.RunAsync(context);

            // Create and start 5 client tasks to send UDP messages concurrently
            List<Task> clientTasks = [];
            for (int i = 0; i < 5; i++)
            {
                var message = $"test123_{i}";
                clientTasks.Add(Task.Run(async () =>
                {
                    await Task.Delay(1000); // Ensure the router has started
                    await SendUdpMessageAsync(message, routerIpEndpoint);
                }));
            }

            // Wait for the router task and all client tasks to complete
            List<Task> allTasks = new(clientTasks)
            {
                routerTask
            };
            await Task.WhenAll(allTasks);

            // Check that all messages have been processed
            commandRouter.RouteCalled.Should().BeTrue();
            commandRouter.MultipleRawString.Distinct().Count().Should().Be(5);
            for (int i = 0; i < 5; i++)
            {
                commandRouter.MultipleRawString.Should().Contain($"test123_{i}");
            }

            exceptionHandler.Called.Should().BeFalse();
        }

        [Fact]
        public async Task Router_Cancels_Correctly()
        {
            var routerPort = FreeTcpPort();
            var routerIpEndpoint = new IPEndPoint(IPAddress.Loopback, routerPort);
            var context = new TerminalUdpRouterContext(routerIpEndpoint, startContext);

            // Cancel after 2 secs
            terminalTokenSource.CancelAfter(2000);

            Task routerTask = udpRouter.RunAsync(context);
            Task clientTask = Task.Run(async () =>
            {
                await Task.Delay(1000); // Wait for router to start
                await SendUdpMessageAsync("test123", routerIpEndpoint);
            });

            // Wait for both the routing task and the client task to complete
            await Task.WhenAll(routerTask, clientTask);

            commandRouter.RouteCalled.Should().BeTrue();

            // Verify logs for cancellation
            loggerFactory.AllLogMessages.Count.Should().Be(7);
            loggerFactory.AllLogMessages[0].Should().Be($"Terminal UDP router started. endpoint={routerIpEndpoint}");
            loggerFactory.AllLogMessages[1].Should().StartWith($"UDP data packet added to command queue. remote="); // UDP Remote IP is dynamic
            loggerFactory.AllLogMessages[1].Should().EndWith("data=test123");
            loggerFactory.AllLogMessages[2].Should().Be($"Routing the command. raw=test123");
            loggerFactory.AllLogMessages[3].Should().Be("Command queue processing cancelled.");
            loggerFactory.AllLogMessages[4].Should().Be("Terminal UDP router cancelled.");
            loggerFactory.AllLogMessages[5].Should().Be("Command processing task completed.");
            loggerFactory.AllLogMessages[6].Should().Be($"Terminal UDP router stopped. endpoint={routerIpEndpoint}");

            exceptionHandler.Called.Should().BeFalse();
        }

        public static int FreeTcpPort()
        {
            TcpListener l = new(IPAddress.Loopback, 0);
            l.Start();
            int port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            return port;
        }

        private async Task SendUdpMessageAsync(string message, IPEndPoint iPEndPoint)
        {
            using (var udpClient = new UdpClient())
            {
                var bytes = textHandler.Encoding.GetBytes(message);
                await udpClient.SendAsync(bytes, bytes.Length, iPEndPoint);
            }
        }

        private MockCommandRouter commandRouter;
        private MockExceptionPublisher exceptionHandler;
        private TerminalOptions options;
        private TerminalUdpRouter udpRouter;
        private TerminalAsciiTextHandler textHandler;
        private ILogger<TerminalUdpRouter> logger;
        private MockListLoggerFactory loggerFactory;
        private TerminalStartContext startContext;
        private CancellationTokenSource terminalTokenSource;
        private CancellationTokenSource commandTokenSource;
    }
}