/*
    Copyright © 2019-2024 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Microsoft.Extensions.Logging;
using OneImlx.Terminal.Configuration.Options;
using OneImlx.Terminal.Mocks;
using OneImlx.Test.FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace OneImlx.Terminal.Runtime.Tests
{
    public class TerminalTcpRouterTests
    {
        public TerminalTcpRouterTests()
        {
            commandRouter = new MockCommandRouter();
            exceptionHandler = new MockExceptionPublisher();
            options = MockTerminalOptions.NewAliasOptions();
            textHandler = new TerminalAsciiTextHandler();
            loggerFactory = new MockListLoggerFactory();
            logger = loggerFactory.CreateLogger<TerminalTcpRouter>();

            terminalTokenSource = new();
            commandTokenSource = new();
            startContext = new TerminalStartContext(TerminalStartMode.Tcp, terminalTokenSource.Token, commandTokenSource.Token, null, null);
            tcpRouter = new TerminalTcpRouter(options, commandRouter, textHandler, exceptionHandler, logger);
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
        public async Task Router_Cancels_Correctly()
        {
            var routerPort = FreeTcpPort();
            var routerIpEndpoint = new IPEndPoint(IPAddress.Loopback, routerPort);
            var context = new TerminalTcpRouterContext(routerIpEndpoint, startContext);

            // Cancel after 2 secs
            terminalTokenSource.CancelAfter(2000);

            Task routerTask = tcpRouter.RunAsync(context);
            Task clientTask = Task.Run(async () =>
            {
                await Task.Delay(1000); // Wait for router to start
                await SendTcpMessageAsync("test123", routerIpEndpoint);
            });

            // Wait for both the routing task and the client task to complete
            await Task.WhenAll(routerTask, clientTask);

            commandRouter.RouteCalled.Should().BeTrue();

            // Verify logs for cancellation
            loggerFactory.AllLogMessages.First().Should().Be($"Terminal TCP routing started. endpoint={routerIpEndpoint}");
            loggerFactory.AllLogMessages.Last().Should().Be($"Terminal TCP routing stopped. endpoint={routerIpEndpoint}");

            exceptionHandler.Called.Should().BeTrue();
            var processedMessages = exceptionHandler.MultiplePublishedMessages.Distinct();
            processedMessages.Should().HaveCount(1);
            processedMessages.Should().Contain("The operation was canceled.");
        }

        [Fact]
        public async Task Router_Errors_If_Message_Length_Greater_Than_Configured()
        {
            options.Router.RemoteMessageMaxLength = 5;

            var routerPort = FreeTcpPort();
            var routerIpEndpoint = new IPEndPoint(IPAddress.Loopback, routerPort);

            var context = new TerminalTcpRouterContext(routerIpEndpoint, startContext);

            // Cancel after 2 secs
            terminalTokenSource.CancelAfter(2000);

            Task routerTask = tcpRouter.RunAsync(context);
            Task clientTask = Task.Run(async () =>
            {
                await Task.Delay(1000); // Wait for router to start
                await SendTcpMessageAsync("test123_more_than_5", routerIpEndpoint);
            });

            // Wait for both the routing task and the client task to complete
            await Task.WhenAll(routerTask, clientTask);

            commandRouter.RouteCalled.Should().BeFalse();

            exceptionHandler.Called.Should().BeTrue();
            var processedMessages = exceptionHandler.MultiplePublishedMessages.Distinct();
            processedMessages.Should().HaveCount(2);
            processedMessages.Should().Contain("The operation was canceled.");
            processedMessages.Should().Contain("The message length is more than configured limit. max_length=5");
        }

        [Fact]
        public async Task Router_Processes_CommandRoute_Exception_Correctly()
        {
            // Throw exception
            commandRouter = new MockCommandRouter(exception: new("Test exception"));
            tcpRouter = new TerminalTcpRouter(options, commandRouter, textHandler, exceptionHandler, logger);

            var routerPort = FreeTcpPort();
            var routerIpEndpoint = new IPEndPoint(IPAddress.Loopback, routerPort);

            var context = new TerminalTcpRouterContext(routerIpEndpoint, startContext);

            // Cancel after 2 secs
            terminalTokenSource.CancelAfter(2000);

            Task routerTask = tcpRouter.RunAsync(context);
            Task clientTask = Task.Run(async () =>
            {
                await Task.Delay(1000); // Wait for router to start
                await SendTcpMessageAsync("test123", routerIpEndpoint);
            });

            // Wait for both the routing task and the client task to complete
            await Task.WhenAll(routerTask, clientTask);

            commandRouter.RouteCalled.Should().BeTrue();
            commandRouter.MultipleRawString.Count.Should().Be(1);
            commandRouter.MultipleRawString[0].Should().Be("test123");

            exceptionHandler.Called.Should().BeTrue();
            var processedMessages = exceptionHandler.MultiplePublishedMessages.Distinct();
            processedMessages.Should().HaveCount(2);
            processedMessages.Should().Contain("The operation was canceled.");
            processedMessages.Should().Contain("Test exception");
        }

        [Fact]
        public async Task Router_Processes_CommandRoute_Timeout_Correctly()
        {
            // Throw timeout
            commandRouter = new MockCommandRouter(routeDelay: 5000);
            tcpRouter = new TerminalTcpRouter(options, commandRouter, textHandler, exceptionHandler, logger);

            var routerPort = FreeTcpPort();
            var routerIpEndpoint = new IPEndPoint(IPAddress.Loopback, routerPort);

            var context = new TerminalTcpRouterContext(routerIpEndpoint, startContext);

            // Cancel after 2 secs
            terminalTokenSource.CancelAfter(2000);

            Task routerTask = tcpRouter.RunAsync(context);
            Task clientTask = Task.Run(async () =>
            {
                await Task.Delay(1000); // Wait for router to start
                await SendTcpMessageAsync("test123", routerIpEndpoint);
            });

            // Wait for both the routing task and the client task to complete
            await Task.WhenAll(routerTask, clientTask);

            commandRouter.RouteCalled.Should().BeTrue();

            exceptionHandler.Called.Should().BeTrue();
            var processedMessages = exceptionHandler.MultiplePublishedMessages.Distinct();
            processedMessages.Should().HaveCount(2);
            processedMessages.Should().Contain("The operation was canceled.");
            processedMessages.Should().Contain("The command router timed out in 25000 milliseconds.");
        }

        [Fact]
        public async Task Router_Processes_Multiple_Messages_From_Single_Client_Correctly()
        {
            int routerPort = FreeTcpPort();
            IPEndPoint routerIpEndpoint = new(IPAddress.Loopback, routerPort);

            var context = new TerminalTcpRouterContext(routerIpEndpoint, startContext);

            // Cancel after 3 seconds to give enough time for all messages to be processed
            terminalTokenSource.CancelAfter(3000);

            // Start the router
            Task routerTask = tcpRouter.RunAsync(context);

            // Create and start a single client task to send 5 messages consecutively
            Task clientTask = Task.Run(async () =>
            {
                // Ensure the router has started
                await Task.Delay(1000);
                for (int i = 0; i < 5; i++)
                {
                    var message = $"test123_{i}";
                    await SendTcpMessageAsync(message, routerIpEndpoint);

                    // Optionally, add a short delay between messages if needed
                    await Task.Delay(100);
                }
            });

            // Wait for the router task and the client task to complete
            await Task.WhenAll(routerTask, clientTask);

            // Check that all messages have been processed
            commandRouter.RouteCalled.Should().BeTrue();
            commandRouter.MultipleRawString.Distinct().Count().Should().Be(5);
            for (int i = 0; i < 5; i++)
            {
                commandRouter.MultipleRawString.Should().Contain($"test123_{i}");
            }

            // Verify no exceptions were published
            exceptionHandler.Called.Should().BeTrue();
            exceptionHandler.MultiplePublishedMessages.Should().AllBe("The operation was canceled.");

            // Verify the context, tcp adds sender_id in properties
            commandRouter.PassedContext!.Properties.Should().NotBeNull();
            commandRouter.PassedContext.Properties!.Count.Should().Be(2);
            commandRouter.PassedContext.Properties.Should().ContainKey(TerminalIdentifiers.SenderEndpointToken);
            commandRouter.PassedContext.Properties.Should().ContainKey(TerminalIdentifiers.SenderIdToken);
        }

        [Fact]
        public async Task Router_Processes_Multiple_Received_Tcp_DelimitedMessages_Correctly()
        {
            options.Router.EnableRemoteDelimiters = true;

            var routerPort = FreeTcpPort();
            var routerIpEndpoint = new IPEndPoint(IPAddress.Loopback, routerPort);

            var context = new TerminalTcpRouterContext(routerIpEndpoint, startContext);

            // Cancel after 3 seconds to give enough time for messages to be processed
            terminalTokenSource.CancelAfter(3000);

            Task routerTask = tcpRouter.RunAsync(context);

            // Create and start 5 client tasks to send TCP messages concurrently
            List<Task> clientTasks = new();
            for (int i = 0; i < 5; i++)
            {
                var message = TerminalServices.DelimitedMessage(options, $"test123_{i}", $"test456_{i}");
                clientTasks.Add(Task.Run(async () =>
                {
                    await Task.Delay(1000); // Ensure the router has started
                    await SendTcpMessageAsync(message, routerIpEndpoint);
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
            commandRouter.MultipleRawString.Count.Should().Be(10);

            // Further assert that the processed commands match the expected values
            var expectedCommands = new List<string>();
            for (int i = 0; i < 5; i++)
            {
                expectedCommands.Add($"test123_{i}");
                expectedCommands.Add($"test456_{i}");
            }

            // Verify that each expected command is present in the processed commands
            foreach (var expectedCommand in expectedCommands)
            {
                commandRouter.MultipleRawString.Should().Contain(expectedCommand);
            }

            exceptionHandler.Called.Should().BeTrue();
            exceptionHandler.MultiplePublishedMessages.Should().AllBe("The operation was canceled.");
        }

        [Fact]
        public async Task Router_Processes_Multiple_Received_Tcp_Messages_Correctly()
        {
            var routerPort = FreeTcpPort();
            var routerIpEndpoint = new IPEndPoint(IPAddress.Loopback, routerPort);

            var context = new TerminalTcpRouterContext(routerIpEndpoint, startContext);

            // Cancel after 3 seconds to give enough time for messages to be processed
            terminalTokenSource.CancelAfter(3000);

            Task routerTask = tcpRouter.RunAsync(context);

            // Create and start 5 client tasks to send TCP messages concurrently
            List<Task> clientTasks = new();
            for (int i = 0; i < 5; i++)
            {
                var message = $"test123_{i}";
                clientTasks.Add(Task.Run(async () =>
                {
                    await Task.Delay(1000); // Ensure the router has started
                    await SendTcpMessageAsync(message, routerIpEndpoint);
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

            exceptionHandler.Called.Should().BeTrue();
            exceptionHandler.MultiplePublishedMessages.Should().AllBe("The operation was canceled.");
        }

        [Fact]
        public async Task Router_Processes_Received_Tcp_Delimited_Message_Correctly()
        {
            options.Router.EnableRemoteDelimiters = true;

            var routerPort = FreeTcpPort();
            var routerIpEndpoint = new IPEndPoint(IPAddress.Loopback, routerPort);

            var context = new TerminalTcpRouterContext(routerIpEndpoint, startContext);

            // Cancel after 2 secs
            terminalTokenSource.CancelAfter(3000);

            Task routerTask = tcpRouter.RunAsync(context);
            Task clientTask = Task.Run(async () =>
            {
                await Task.Delay(1000); // Wait for router to start
                await SendTcpMessageAsync(TerminalServices.DelimitedMessage(options, "test123", "test456", "test789"), routerIpEndpoint);
            });

            // Wait for both the routing task and the client task to complete
            await Task.WhenAll(routerTask, clientTask);

            commandRouter.RouteCalled.Should().BeTrue();
            commandRouter.MultipleRawString.Count.Should().Be(3);
            commandRouter.MultipleRawString[0].Should().Be("test123");
            commandRouter.MultipleRawString[1].Should().Be("test456");
            commandRouter.MultipleRawString[2].Should().Be("test789");

            exceptionHandler.Called.Should().BeTrue();
            exceptionHandler.MultiplePublishedMessages.Should().AllBe("The operation was canceled.");
        }

        [Fact]
        public async Task Router_Processes_Received_Tcp_Message_Correctly()
        {
            var routerPort = FreeTcpPort();
            var routerIpEndpoint = new IPEndPoint(IPAddress.Loopback, routerPort);

            var context = new TerminalTcpRouterContext(routerIpEndpoint, startContext);

            // Cancel after 2 secs
            terminalTokenSource.CancelAfter(2000);

            Task routerTask = tcpRouter.RunAsync(context);
            Task clientTask = Task.Run(async () =>
            {
                await Task.Delay(1000); // Wait for router to start
                await SendTcpMessageAsync("test123", routerIpEndpoint);
            });

            // Wait for both the routing task and the client task to complete
            await Task.WhenAll(routerTask, clientTask);

            commandRouter.RouteCalled.Should().BeTrue();
            commandRouter.MultipleRawString.Count.Should().Be(1);
            commandRouter.MultipleRawString[0].Should().Be("test123");

            exceptionHandler.Called.Should().BeTrue();
            exceptionHandler.MultiplePublishedMessages.Should().AllBe("The operation was canceled.");
        }

        [Fact]
        public async Task Router_Starts_And_Stops_Correctly()
        {
            var routerPort = FreeTcpPort();
            var routerIpEndpoint = new IPEndPoint(IPAddress.Loopback, routerPort);
            var context = new TerminalTcpRouterContext(routerIpEndpoint, startContext);

            // Use a short cancel delay to just start and then stop the server
            terminalTokenSource.CancelAfter(1500); // Adjust the delay as needed

            // Run the router task and wait for completion
            Task routerTask = tcpRouter.RunAsync(context);
            await routerTask;

            // Verify that the router has started correctly
            loggerFactory.AllLogMessages.First().Should().Be($"Terminal TCP routing started. endpoint={routerIpEndpoint}");
            loggerFactory.AllLogMessages.Last().Should().Be($"Terminal TCP routing stopped. endpoint={routerIpEndpoint}");

            // Wait a moment for any potential additional processing
            await Task.Delay(500);
            routerTask.IsCompleted.Should().BeTrue();
        }

        [Fact]
        public async Task Router_Throws_If_EndPoint_Null()
        {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            var context = new TerminalTcpRouterContext(null, startContext);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            Func<Task> routerTask = async () => await tcpRouter.RunAsync(context);
            await routerTask.Should().ThrowAsync<TerminalException>()
                .WithErrorCode("invalid_configuration")
                .WithErrorDescription("The network IP endpoint is missing in the TCP server routing request.");
        }

        [Fact]
        public async Task Router_Throws_If_Not_Tcp()
        {
            // Not TCP
            startContext = new TerminalStartContext(TerminalStartMode.Grpc, terminalTokenSource.Token, commandTokenSource.Token, null, null);

            var routerPort = FreeTcpPort();
            var routerIpEndpoint = new IPEndPoint(IPAddress.Loopback, routerPort);

            var context = new TerminalTcpRouterContext(routerIpEndpoint, startContext);

            Func<Task> routerTask = async () => await tcpRouter.RunAsync(context);
            await routerTask.Should().ThrowAsync<TerminalException>()
                .WithErrorCode("invalid_configuration")
                .WithErrorDescription("The requested start mode is not valid for TCP routing. start_mode=Grpc");
        }

        private async Task SendTcpMessageAsync(string message, IPEndPoint iPEndPoint)
        {
            using (var tcpClient = new TcpClient())
            {
                await tcpClient.ConnectAsync(iPEndPoint.Address, iPEndPoint.Port);
                using (var stream = tcpClient.GetStream())
                {
                    var bytes = textHandler.Encoding.GetBytes(message);
                    await stream.WriteAsync(bytes);
                }
            }
        }

        private readonly CancellationTokenSource commandTokenSource;
        private readonly MockExceptionPublisher exceptionHandler;
        private readonly ILogger<TerminalTcpRouter> logger;
        private readonly MockListLoggerFactory loggerFactory;
        private readonly TerminalOptions options;
        private readonly CancellationTokenSource terminalTokenSource;
        private readonly TerminalAsciiTextHandler textHandler;
        private MockCommandRouter commandRouter;
        private TerminalStartContext startContext;
        private TerminalTcpRouter tcpRouter;
    }
}
