/*
    Copyright 2024 (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using FluentAssertions;
using OneImlx.Terminal.Commands.Routers;
using OneImlx.Terminal.Configuration.Options;
using OneImlx.Terminal.Mocks;
using OneImlx.Terminal.Runtime;
using Serilog;
using Xunit;

namespace OneImlx.Terminal.Extensions
{
    [Collection("Sequential")]
    public class IHostExtensionsRunTcpRoutingTests : IAsyncLifetime
    {
        public Task DisposeAsync()
        {
            host?.Dispose();
            return Task.CompletedTask;
        }

        [Fact]
        public async Task HandleClientConnected_Handles_Delimited_Messages_CorrectlyAsync()
        {
            host = CreateHostWithLogger(ConfigureServicesDefault, nameof(HandleClientConnected_Handles_Delimited_Messages_CorrectlyAsync));
            TerminalOptions options = GetOptions(host);
            options.Router.EnableRemoteDelimiters = true;

            // Start the TCP routing asynchronously
            var routingTask = host.RunTerminalRouterAsync<TerminalTcpRouter, TerminalTcpRouterContext>(new TerminalTcpRouterContext(serverIpEndPoint, startContext));

            // Start the client task
            var clientTask = Task.Run(async () =>
            {
                await Task.Delay(1000); // Wait for 1 second
                using var tcpClient = new TcpClient();
                await tcpClient.ConnectAsync(serverIpEndPoint.Address, serverIpEndPoint.Port);

                // Send a command that exceeds the configured command string length limit
                string testString = TerminalServices.DelimitedMessage(options, "rt1 grp1 cmd1", "rt2 grp2 cmd2", "rt3 grp3 cmd3");

                byte[] messageBytes = textHandler.Encoding.GetBytes(testString);
                await tcpClient.GetStream().WriteAsync(messageBytes, 0, messageBytes.Length);
                await tcpClient.GetStream().FlushAsync();
            });

            // Cancel the token source to stop the server and client
            terminalTokenSource.CancelAfter(2000);

            // Wait for both the routing task and the client task to complete
            await Task.WhenAll(routingTask, clientTask);

            // Assert
            MockCommandRouter mockCommandRouter = (MockCommandRouter)host.Services.GetRequiredService<ICommandRouter>();
            mockCommandRouter.RouteCalled.Should().BeTrue();
            mockCommandRouter.MultipleRawString.Count.Should().Be(3);
            mockCommandRouter.MultipleRawString[0].Should().Be("rt1 grp1 cmd1");
            mockCommandRouter.MultipleRawString[1].Should().Be("rt2 grp2 cmd2");
            mockCommandRouter.MultipleRawString[2].Should().Be("rt3 grp3 cmd3");

            // Verify that the server and client tasks have completed
            routingTask.Status.Should().Be(TaskStatus.RanToCompletion, because: "The server task should have completed successfully.");
            clientTask.Status.Should().Be(TaskStatus.RanToCompletion, because: "The client task should have completed successfully.");
        }

        [Fact]
        public async Task HandleClientConnected_Handles_Large_Delimited_Messages_CorrectlyAsync()
        {
            host = CreateHostWithLogger(ConfigureServicesDefault, nameof(HandleClientConnected_Handles_Large_Delimited_Messages_CorrectlyAsync));
            TerminalOptions options = GetOptions(host);
            options.Router.EnableRemoteDelimiters = true;
            options.Router.RemoteMessageMaxLength = 100000;

            // Start the TCP routing asynchronously
            var routingTask = host.RunTerminalRouterAsync<TerminalTcpRouter, TerminalTcpRouterContext>(new TerminalTcpRouterContext(serverIpEndPoint, startContext));

            // Start the client task
            var clientTask = Task.Run(async () =>
            {
                await Task.Delay(1000); // Wait for 1 second
                using var tcpClient = new TcpClient();
                await tcpClient.ConnectAsync(serverIpEndPoint.Address, serverIpEndPoint.Port);

                // Create a large command string with 100 commands
                List<string> commandList = new();
                for (int i = 0; i < 100; i++)
                {
                    commandList.Add($"rt{i} grp{i} cmd{i}");
                }
                string testString = TerminalServices.DelimitedMessage(options, commandList.ToArray());

                byte[] messageBytes = textHandler.Encoding.GetBytes(testString);
                await tcpClient.GetStream().WriteAsync(messageBytes);
                await tcpClient.GetStream().FlushAsync();
            });

            // Cancel the token source to stop the server and client
            terminalTokenSource.CancelAfter(2000);

            // Wait for both the routing task and the client task to complete
            await Task.WhenAll(routingTask, clientTask);

            // Assert
            MockCommandRouter mockCommandRouter = (MockCommandRouter)host.Services.GetRequiredService<ICommandRouter>();
            mockCommandRouter.RouteCalled.Should().BeTrue();
            mockCommandRouter.MultipleRawString.Count.Should().Be(100);
            for (int idx = 0; idx < 100; idx++)
            {
                mockCommandRouter.MultipleRawString[idx].Should().Be($"rt{idx} grp{idx} cmd{idx}");
            }

            // Verify that the server and client tasks have completed
            routingTask.Status.Should().Be(TaskStatus.RanToCompletion, because: "The server task should have completed successfully.");
            clientTask.Status.Should().Be(TaskStatus.RanToCompletion, because: "The client task should have completed successfully.");
        }

        [Fact]
        public async Task HandleClientConnected_Handles_Multiple_Delimited_Messages_CorrectlyAsync()
        {
            host = CreateHostWithLogger(ConfigureServicesDefault, nameof(HandleClientConnected_Handles_Multiple_Delimited_Messages_CorrectlyAsync));
            TerminalOptions options = GetOptions(host);
            options.Router.EnableRemoteDelimiters = true;

            // Start the TCP routing asynchronously
            var routingTask = host.RunTerminalRouterAsync<TerminalTcpRouter, TerminalTcpRouterContext>(new TerminalTcpRouterContext(serverIpEndPoint, startContext));

            // Start the client task
            var clientTask = Task.Run(async () =>
            {
                await Task.Delay(1000); // Wait for 1 second
                using var tcpClient = new TcpClient();
                await tcpClient.ConnectAsync(serverIpEndPoint.Address, serverIpEndPoint.Port);

                // Send a command that exceeds the configured command string length limit
                string testString = TerminalServices.DelimitedMessage(options, "rt1 grp1 cmd1", "rt2 grp2 cmd2", "rt3 grp3 cmd3");
                byte[] messageBytes = textHandler.Encoding.GetBytes(testString);
                await tcpClient.GetStream().WriteAsync(messageBytes);
                await tcpClient.GetStream().FlushAsync();

                await Task.Delay(3000);

                // Send a command that exceeds the configured command string length limit
                testString = TerminalServices.DelimitedMessage(GetOptions(host), "rt4 grp4 cmd4", "rt5 grp5 cmd5", "rt6 grp6 cmd6");
                messageBytes = textHandler.Encoding.GetBytes(testString);
                await tcpClient.GetStream().WriteAsync(messageBytes);
                await tcpClient.GetStream().FlushAsync();
            });

            // Cancel the token source to stop the server and client
            terminalTokenSource.CancelAfter(4500);

            // Wait for both the routing task and the client task to complete
            await Task.WhenAll(routingTask, clientTask);

            // Assert
            MockCommandRouter mockCommandRouter = (MockCommandRouter)host.Services.GetRequiredService<ICommandRouter>();
            mockCommandRouter.RouteCalled.Should().BeTrue();
            mockCommandRouter.MultipleRawString.Count.Should().Be(6);
            mockCommandRouter.MultipleRawString[0].Should().Be("rt1 grp1 cmd1");
            mockCommandRouter.MultipleRawString[1].Should().Be("rt2 grp2 cmd2");
            mockCommandRouter.MultipleRawString[2].Should().Be("rt3 grp3 cmd3");
            mockCommandRouter.MultipleRawString[3].Should().Be("rt4 grp4 cmd4");
            mockCommandRouter.MultipleRawString[4].Should().Be("rt5 grp5 cmd5");
            mockCommandRouter.MultipleRawString[5].Should().Be("rt6 grp6 cmd6");

            // Verify that the server and client tasks have completed
            routingTask.Status.Should().Be(TaskStatus.RanToCompletion, because: "The server task should have completed successfully.");
            clientTask.Status.Should().Be(TaskStatus.RanToCompletion, because: "The client task should have completed successfully.");
        }

        [Fact]
        public async Task HandleClientConnected_Handles_Very_Large_Delimited_Messages_CorrectlyAsync()
        {
            host = CreateHostWithLogger(ConfigureServicesDefault, nameof(HandleClientConnected_Handles_Very_Large_Delimited_Messages_CorrectlyAsync));
            TerminalOptions options = GetOptions(host);
            options.Router.EnableRemoteDelimiters = true;
            options.Router.RemoteMessageMaxLength = 100000;

            // Start the TCP routing asynchronously
            var routingTask = host.RunTerminalRouterAsync<TerminalTcpRouter, TerminalTcpRouterContext>(new TerminalTcpRouterContext(serverIpEndPoint, startContext));

            // Start the client task
            var clientTask = Task.Run(async () =>
            {
                await Task.Delay(1000); // Wait for 1 second
                using var tcpClient = new TcpClient();
                await tcpClient.ConnectAsync(serverIpEndPoint.Address, serverIpEndPoint.Port);

                // Create a large command string with 1000 commands
                List<string> commandList = [];
                for (int i = 0; i < 1000; i++)
                {
                    commandList.Add($"rt{i} grp{i} cmd{i}");
                }
                string testString = TerminalServices.DelimitedMessage(options, commandList.ToArray());

                byte[] messageBytes = textHandler.Encoding.GetBytes(testString);
                await tcpClient.GetStream().WriteAsync(messageBytes, 0, messageBytes.Length);
                await tcpClient.GetStream().FlushAsync();
            });

            // Cancel the token source to stop the server and client
            terminalTokenSource.CancelAfter(5000);

            // Wait for both the routing task and the client task to complete
            await Task.WhenAll(routingTask, clientTask);

            // Assert
            MockCommandRouter mockCommandRouter = (MockCommandRouter)host.Services.GetRequiredService<ICommandRouter>();
            mockCommandRouter.RouteCalled.Should().BeTrue();
            mockCommandRouter.MultipleRawString.Count.Should().Be(1000);
            for (int idx = 0; idx < 1000; idx++)
            {
                mockCommandRouter.MultipleRawString[idx].Should().Be($"rt{idx} grp{idx} cmd{idx}");
            }

            // Verify that the server and client tasks have completed
            routingTask.Status.Should().Be(TaskStatus.RanToCompletion, because: "The server task should have completed successfully.");
            clientTask.Status.Should().Be(TaskStatus.RanToCompletion, because: "The client task should have completed successfully.");
        }

        [Fact]
        public async Task HandleClientConnected_Should_Handle_Malformed_Delimited_Messages_Async()
        {
            host = CreateHostWithLogger(ConfigureServicesDefault, nameof(HandleClientConnected_Should_Handle_Malformed_Delimited_Messages_Async));

            GetOptions(host).Router.Timeout = Timeout.Infinite;

            // Cancel so that the client does not go into an infinite loop during stream.Read
            terminalTokenSource.CancelAfter(2000);

            // Start the TCP routing asynchronously
            var routingTask = host.RunTerminalRouterAsync<TerminalTcpRouter, TerminalTcpRouterContext>(new TerminalTcpRouterContext(serverIpEndPoint, startContext));

            // Simulate a client connecting to the server after a short delay and sending a malformed delimited message
            var connectTask = Task.Run(async () =>
            {
                await Task.Delay(1000); // Wait for 1000 milliseconds before connecting
                using var tcpClient = new TcpClient();
                await tcpClient.ConnectAsync(serverIpEndPoint.Address, serverIpEndPoint.Port);

                // Send a malformed delimited message (without proper delimiters)
                byte[] messageBytes = textHandler.Encoding.GetBytes("Malformed message without delimiters");
                await tcpClient.GetStream().WriteAsync(messageBytes, 0, messageBytes.Length);
                await tcpClient.GetStream().FlushAsync();
            });

            // Act Wait for both routingTask and connectTask to complete
            await Task.WhenAll(routingTask, connectTask);

            // Assert Check if the client is connected and the server handles the malformed message correctly
            routingTask.Status.Should().Be(TaskStatus.RanToCompletion, because: "The server task should have completed successfully.");
            connectTask.Status.Should().Be(TaskStatus.RanToCompletion, because: "The client task should have completed successfully.");
        }

        [Fact]
        public async Task HandleClientConnected_Should_Route_Command_To_Router()
        {
            host = CreateHostWithLogger(ConfigureServicesDefault, nameof(HandleClientConnected_Should_Route_Command_To_Router));

            // Start the TCP routing asynchronously
            var routingTask = host.RunTerminalRouterAsync<TerminalTcpRouter, TerminalTcpRouterContext>(new TerminalTcpRouterContext(serverIpEndPoint, startContext));

            terminalTokenSource.CancelAfter(2000); // Wait for 2 seconds and then cancel the server

            // Start the client task
            var clientTask = Task.Run(async () =>
            {
                await Task.Delay(1000); // Wait for 1 second
                using var tcpClient = new TcpClient();
                await tcpClient.ConnectAsync(serverIpEndPoint.Address, serverIpEndPoint.Port);

                // Send a command from the client to the server
                byte[] messageBytes = textHandler.Encoding.GetBytes("Test command");
                await tcpClient.GetStream().WriteAsync(messageBytes);
                await tcpClient.GetStream().FlushAsync();
            });

            // Wait for both the routing task and the client task to complete
            await Task.WhenAll(routingTask, clientTask);

            // Assert
            MockCommandRouter mockCommandRouter = (MockCommandRouter)host.Services.GetRequiredService<ICommandRouter>();
            mockCommandRouter.RouteCalled.Should().BeTrue();
            mockCommandRouter.RawCommandString.Should().Be("Test command");

            // Verify that the server and client tasks have completed
            await routingTask;
            await clientTask;
            routingTask.Status.Should().Be(TaskStatus.RanToCompletion, because: "The server task should have completed successfully.");
            clientTask.Status.Should().Be(TaskStatus.RanToCompletion, because: "The client task should have completed successfully.");
        }

        [Fact]
        public async Task HandleClientConnected_Throw_Exception_If_PackageData_Exceeds_LimitAsync()
        {
            host = CreateHostWithLogger(ConfigureServicesDefault, nameof(HandleClientConnected_Throw_Exception_If_PackageData_Exceeds_LimitAsync));

            // Start the TCP routing asynchronously
            var routingTask = host.RunTerminalRouterAsync<TerminalTcpRouter, TerminalTcpRouterContext>(new TerminalTcpRouterContext(serverIpEndPoint, startContext));

            // Start the client task
            var clientTask = Task.Run(async () =>
            {
                await Task.Delay(1000); // Wait for 1 second
                using var tcpClient = new TcpClient();
                await tcpClient.ConnectAsync(serverIpEndPoint.Address, serverIpEndPoint.Port);

                // Send a command that exceeds the configured command string length limit
                byte[] messageBytes = textHandler.Encoding.GetBytes(new string('A', 10000)); // Length exceeds the limit
                await tcpClient.GetStream().WriteAsync(messageBytes, 0, messageBytes.Length);
                await tcpClient.GetStream().FlushAsync();
            });

            // Client handling throws an exception
            await Task.WhenAny(routingTask, clientTask);
            routingTask.Status.Should().NotBe(TaskStatus.RanToCompletion, because: "The server task should not be completed successfully.");
            clientTask.Status.Should().Be(TaskStatus.RanToCompletion, because: "The client task should be completed successfully.");

            // Wait for .5 seconds and make sure tasks are completed
            await Task.Delay(2000);

            // Check the published error, task id is variable
            MockExceptionPublisher exPublisher = (MockExceptionPublisher)host.Services.GetRequiredService<ITerminalExceptionHandler>();
            exPublisher.Called.Should().BeTrue();
            exPublisher.PublishedMessage.Should().Be("The message length is more than configured limit. max_length=1024");

            // Cancel the token source to stop the server and client (if not already completed)
            terminalTokenSource.Cancel();

            // Wait for both the routing task and the client task to complete
            await Task.WhenAll(routingTask, clientTask);
            routingTask.Status.Should().Be(TaskStatus.RanToCompletion, because: "The server task should have completed successfully.");
            clientTask.Status.Should().Be(TaskStatus.RanToCompletion, because: "The server task should have completed successfully.");
        }

        public Task InitializeAsync()
        {
            serverIpEndPoint = new IPEndPoint(IPAddress.Loopback, 12345);

            return Task.CompletedTask;
        }

        [Fact]
        public async Task Multiple_Clients_Should_Send_Commands_Concurrently()
        {
            // Arrange
            host = CreateHostWithLogger(ConfigureServicesDefault, nameof(Multiple_Clients_Should_Send_Commands_Concurrently));

            // Set the timeout to infinite to avoid cancellation during the test
            GetOptions(host).Router.Timeout = Timeout.Infinite;

            // Start the TCP routing asynchronously
            var routingTask = host.RunTerminalRouterAsync<TerminalTcpRouter, TerminalTcpRouterContext>(new TerminalTcpRouterContext(serverIpEndPoint, startContext));

            // Start multiple client tasks and send a valid delimited message
            const int numClients = 5;
            Task[] _clientTasks = new Task[numClients];
            var multipleClientsTask = Task.Run(async () =>
            {
                await Task.Delay(1000); // Wait for 1 second

                for (int idx = 0; idx < numClients; idx++)
                {
                    int localIdx = idx;
                    _clientTasks[localIdx] = Task.Run(async () =>
                    {
                        using var tcpClient = new TcpClient();
                        await tcpClient.ConnectAsync(serverIpEndPoint.Address, serverIpEndPoint.Port);
                        tcpClient.Connected.Should().BeTrue();

                        var command = $"Client-{localIdx + 1} sent test command";
                        var commandBytes = textHandler.Encoding.GetBytes(TerminalServices.DelimitedMessage(GetOptions(host), command));
                        await tcpClient.GetStream().WriteAsync(commandBytes, 0, commandBytes.Length);
                        await tcpClient.GetStream().FlushAsync();
                    });
                }
            });

            terminalTokenSource.CancelAfter(6000); // Stop the server

            await Task.WhenAll(routingTask, multipleClientsTask);

            // Verify that all client tasks are completed
            for (int i = 0; i < numClients; i++)
            {
                await _clientTasks[i];
                _clientTasks[i].Status.Should().Be(TaskStatus.RanToCompletion, because: "The client task should have completed successfully.");
            }

            await routingTask; // Wait for the routing task to complete
            routingTask.Status.Should().Be(TaskStatus.RanToCompletion, because: "The server task should have completed successfully.");

            // Assert result
            var mockCommandRouter = (MockCommandRouter)host.Services.GetRequiredService<ICommandRouter>();
            mockCommandRouter.RouteCalled.Should().BeTrue();
            mockCommandRouter.RouteCounter.Should().Be(numClients);
            mockCommandRouter.MultipleRawString.Count.Should().Be(numClients);
            mockCommandRouter.MultipleRawString.Should().OnlyHaveUniqueItems();
        }

        [Fact]
        public async Task RunAsync_Should_Handle_Exception_In_Routing_Process_Async()
        {
            host = CreateHostWithLogger(ConfigureServicesErrorExceptionOnRoute, nameof(RunAsync_Should_Handle_Exception_In_Routing_Process_Async));

            // Set the timeout to infinite to avoid cancellation during the test
            GetOptions(host).Router.Timeout = Timeout.Infinite;

            // Start the TCP routing asynchronously
            var routingTask = host.RunTerminalRouterAsync<TerminalTcpRouter, TerminalTcpRouterContext>(new TerminalTcpRouterContext(serverIpEndPoint, startContext));

            // Start the client task and send a valid delimited message
            var clientTask = Task.Run(async () =>
            {
                await Task.Delay(1000); // Wait for 1 second
                using var tcpClient = new TcpClient();
                await tcpClient.ConnectAsync(serverIpEndPoint.Address, serverIpEndPoint.Port);

                // Send a valid delimited message
                byte[] messageBytes = textHandler.Encoding.GetBytes(TerminalServices.DelimitedMessage(GetOptions(host), "Test command"));
                await tcpClient.GetStream().WriteAsync(messageBytes, 0, messageBytes.Length);
                await tcpClient.GetStream().FlushAsync();
            });

            // Wait for the client task to complete
            await Task.WhenAny(routingTask, clientTask);
            routingTask.Status.Should().NotBe(TaskStatus.RanToCompletion, because: "The server task should not be completed successfully.");
            clientTask.Status.Should().Be(TaskStatus.RanToCompletion, because: "The client task should have completed successfully.");

            // Wait for .5 seconds and make sure tasks are completed
            await Task.Delay(2000);

            // Check the published error
            MockExceptionPublisher exPublisher = (MockExceptionPublisher)host.Services.GetRequiredService<ITerminalExceptionHandler>();
            exPublisher.Called.Should().BeTrue();
            exPublisher.PublishedMessage.Should().Be("test_error_description. opt1=test1 opt2=test2");

            terminalTokenSource.Cancel();
            await Task.WhenAll(routingTask, clientTask);
            routingTask.Status.Should().Be(TaskStatus.RanToCompletion, because: "The server task should have completed successfully.");
            clientTask.Status.Should().Be(TaskStatus.RanToCompletion, because: "The client task should have completed successfully.");
        }

        /// <summary>
        /// Verifies that the server runs indefinitely and stops when the cancellation token is triggered.
        /// </summary>
        /// <remarks>
        /// Test Steps:
        /// <list type="number">
        /// <item>Starts the TCP routing asynchronously by calling <c>host.RunTcpRoutingAsync</c>.</item>
        /// <item>Waits for either <c>routingTask</c> or a 10-second delay using <c>Task.WhenAny</c>.</item>
        /// <item>
        /// Checks that <c>routingTask</c> is not completed after the 10-second wait, confirming that it's running indefinitely.
        /// </item>
        /// <item>Issues a cancellation by calling <c>tokenSource.CancelAfter(2000)</c> to stop the server after 2 seconds.</item>
        /// <item>
        /// Waits for 2.5 seconds using <c>await Task.Delay(2500)</c> to ensure that the cancellation is processed and
        /// <c>routingTask</c> is completed.
        /// </item>
        /// <item>Verifies that <c>routingTask</c> is completed, confirming that the server stopped after the cancellation.</item>
        /// </list>
        /// </remarks>
        [Fact]
        public async Task RunAsync_Should_Run_Indefinitely()
        {
            host = CreateHostWithLogger(ConfigureServicesDefault, nameof(RunAsync_Should_Run_Indefinitely));

            GetOptions(host).Router.Timeout = Timeout.Infinite;

            // Start the TCP routing asynchronously
            var routingTask = host.RunTerminalRouterAsync<TerminalTcpRouter, TerminalTcpRouterContext>(new TerminalTcpRouterContext(serverIpEndPoint, startContext));

            // Wait for 10 seconds and make sure routingTask is still running
            await Task.WhenAny(routingTask, Task.Delay(10000));

            // Server is not yet complete
            routingTask.Status.Should().NotBe(TaskStatus.RanToCompletion, because: "The server task should not be completed successfully.");

            // Stop the server by issuing a cancellation
            terminalTokenSource.CancelAfter(2000);

            // Wait for routingTask to complete
            await routingTask;

            // Verify that the server runs indefinitely and stops when the cancellation token is triggered
            routingTask.Status.Should().Be(TaskStatus.RanToCompletion, because: "The server task should have completed successfully.");
        }

        [Fact]
        public async Task RunAsync_Should_Start_And_Stop_Server()
        {
            host = CreateHostWithLogger(ConfigureServicesDefault, nameof(RunAsync_Should_Start_And_Stop_Server));
            
            // Set the timeout to infinite to avoid cancellation during the test
            GetOptions(host).Router.Timeout = Timeout.Infinite;

            // Start the TCP routing asynchronously with the cancellation token
            var routingTask = host.RunTerminalRouterAsync<TerminalTcpRouter, TerminalTcpRouterContext>(new TerminalTcpRouterContext(serverIpEndPoint, startContext));

            // Cancel the server after running for a while
            terminalTokenSource.CancelAfter(2000); // Wait for 2 seconds and then cancel the server

            // Wait for the server task to complete
            await routingTask;

            // Ensure that the server task has completed successfully
            routingTask.Status.Should().Be(TaskStatus.RanToCompletion, because: "The server task should have completed successfully.");
        }

        [Fact]
        public async Task RunAsync_Should_Stop_Server_And_Client_On_CancellationAsync()
        {
            host = CreateHostWithLogger(ConfigureServicesDefault, nameof(RunAsync_Should_Stop_Server_And_Client_On_CancellationAsync));
            GetOptions(host).Router.Timeout = Timeout.Infinite;

            // Start the TCP routing asynchronously
            var routingTask = host.RunTerminalRouterAsync<TerminalTcpRouter, TerminalTcpRouterContext>(new TerminalTcpRouterContext(serverIpEndPoint, startContext));

            // Start the client task
            var clientTask = Task.Run(async () =>
            {
                await Task.Delay(1000); // Wait for 1 second
                using var tcpClient = new TcpClient();
                await tcpClient.ConnectAsync(serverIpEndPoint.Address, serverIpEndPoint.Port);
            });

            // Cancel both the server and client tasks after a delay
            terminalTokenSource.CancelAfter(3000);

            // Assuming routingTask and clientTask are Tasks
            var delayTask = Task.Delay(5000);
            var completedTask = await Task.WhenAny(
                Task.WhenAll(routingTask, clientTask),
                delayTask
                                                  );

            if (completedTask == delayTask)
            {
                throw new TimeoutException("The operation timed out in 5000 milliseconds.");
            }

            // Verify that both server and client tasks stopped on cancellation
            routingTask.Status.Should().Be(TaskStatus.RanToCompletion, because: "The server task should have completed successfully.");
            clientTask.Status.Should().Be(TaskStatus.RanToCompletion, because: "The client task should have completed successfully.");
        }

        [Fact]
        public async Task RunAsync_Should_Stop_Server_And_Multiple_Clients_On_Cancellation()
        {
            host = CreateHostWithLogger(ConfigureServicesDefault, nameof(RunAsync_Should_Stop_Server_And_Multiple_Clients_On_Cancellation));
            GetOptions(host).Router.Timeout = Timeout.Infinite;

            // Stop the server by issuing a cancellation
            terminalTokenSource.CancelAfter(8000);

            // Start the TCP routing asynchronously
            var routingTask = host.RunTerminalRouterAsync<TerminalTcpRouter, TerminalTcpRouterContext>(new TerminalTcpRouterContext(serverIpEndPoint, startContext));

            // Number of clients to simulate
            int numClients = 5;

            // List to store client tasks
            var clientTasks = new List<Task>();
            for (int i = 0; i < numClients; i++)
            {
                // Simulate a client connecting to the server and sending a unique message
                var clientTask = Task.Run(async () =>
                {
                    await Task.Delay(1000); // Wait for 1 second to stagger the connections
                    using var tcpClient = new TcpClient();
                    await tcpClient.ConnectAsync(serverIpEndPoint.Address, serverIpEndPoint.Port);
                });

                clientTasks.Add(clientTask);
            }

            // Wait for all the client tasks to complete or timeout after 10 seconds
            List<Task> allTasks = new(clientTasks)
            {
                routingTask
            };
            await Task.WhenAll(allTasks);

            // Verify that the server runs indefinitely and stops when the cancellation token is triggered
            routingTask.Status.Should().Be(TaskStatus.RanToCompletion, because: "The server task should have completed successfully.");
            for (int i = 0; i < numClients; i++)
            {
                clientTasks[i].Status.Should().Be(TaskStatus.RanToCompletion, because: "The client task should have completed successfully.");
            }
        }

        [Fact]
        public async Task RunAsync_Should_Stop_Server_On_Cancellation()
        {
            host = CreateHostWithLogger(ConfigureServicesDefault, nameof(RunAsync_Should_Stop_Server_On_Cancellation));
            GetOptions(host).Router.Timeout = Timeout.Infinite;

            // Start the TCP routing asynchronously
            var routingTask = host.RunTerminalRouterAsync<TerminalTcpRouter, TerminalTcpRouterContext>(new TerminalTcpRouterContext(serverIpEndPoint, startContext));

            // Wait for 3 seconds and make sure routingTask is still running
            await Task.WhenAny(routingTask, Task.Delay(3000));

            // Server is not yet complete
            routingTask.Status.Should().NotBe(TaskStatus.RanToCompletion, because: "The server task should not be completed successfully.");

            // Stop the server by issuing a cancellation
            terminalTokenSource.Cancel();

            // Wait for routingTask to complete or timeout after 500 seconds
            if (await Task.WhenAny(routingTask, Task.Delay(500)) != routingTask)
            {
                throw new TimeoutException("The operation timed out after 500 milliseconds.");
            }

            // Verify that the server stops on cancellation
            routingTask.Status.Should().Be(TaskStatus.RanToCompletion, because: "The server task should have completed successfully.");
        }

        [Fact]
        public async Task RunAsync_Should_Throw_Exception_If_IPEndPoint_Is_Null()
        {
            var newHostBuilder = Host.CreateDefaultBuilder().ConfigureServices(ConfigureServicesDefault);
            host = newHostBuilder.Build();

            GetOptions(host).Router.Timeout = Timeout.Infinite;

            Func<Task> act = async () => await host.RunTerminalRouterAsync<TerminalTcpRouter, TerminalTcpRouterContext>(new TerminalTcpRouterContext(null!, startContext));
            await act.Should().ThrowAsync<TerminalException>().WithMessage("The network IP endpoint is missing in the TCP server routing request.");
        }

        [Fact]
        public async Task RunAsync_Should_Throw_InvalidConfiguration_For_Invalid_StartMode()
        {
            startContext = new TerminalStartContext(TerminalStartMode.Custom, terminalTokenSource.Token, commandTokenSource.Token);
            var context = new TerminalTcpRouterContext(serverIpEndPoint, startContext);
            Func<Task> act = async () => await host.RunTerminalRouterAsync<TerminalTcpRouter, TerminalTcpRouterContext>(context);
            await act.Should().ThrowAsync<TerminalException>().WithMessage("The requested start mode is not valid for TCP routing. start_mode=Custom");
        }

        private static IHost CreateHostWithLogger(Action<IServiceCollection> configureServicesDefault, string logName)
        {
            string logFile = Path.ChangeExtension(logName, ".log");

            // Set up the logger configuration
            Log.Logger = new LoggerConfiguration()
               .MinimumLevel.Debug()
               .WriteTo.File(logFile)
               .CreateLogger();

            var newHostBuilder = Host.CreateDefaultBuilder()
                                     .ConfigureServices(configureServicesDefault)
                                     .UseSerilog();

            return newHostBuilder.Build();
        }

        private TerminalOptions GetOptions(IHost host)
        {
            return host.Services.GetRequiredService<TerminalOptions>();
        }

        private void ConfigureServicesDefault(IServiceCollection opt2)
        {
            textHandler = new TerminalAsciiTextHandler();
            terminalTokenSource = new CancellationTokenSource();
            commandTokenSource = new CancellationTokenSource();

            startContext = new TerminalStartContext(TerminalStartMode.Tcp, terminalTokenSource.Token, commandTokenSource.Token);

            opt2.AddSingleton<ICommandRouter>(new MockCommandRouter());
            opt2.AddSingleton(MockTerminalOptions.NewLegacyOptions());

            opt2.AddSingleton<ITerminalExceptionHandler>(new MockExceptionPublisher());
            opt2.AddSingleton<ITerminalRouter<TerminalTcpRouterContext>, TerminalTcpRouter>();
            opt2.AddSingleton(textHandler);
            opt2.AddSingleton<ITerminalConsole, TerminalSystemConsole>();
        }

        private void ConfigureServicesErrorExceptionOnRoute(IServiceCollection opt2)
        {
            textHandler = new TerminalAsciiTextHandler();
            terminalTokenSource = new CancellationTokenSource();
            commandTokenSource = new CancellationTokenSource();

            startContext = new TerminalStartContext(TerminalStartMode.Tcp, terminalTokenSource.Token, commandTokenSource.Token);

            opt2.AddSingleton<ICommandRouter>(new MockCommandRouter(null, null, new TerminalException("test_error_code", "test_error_description. opt1={0} opt2={1}", "test1", "test2")));
            opt2.AddSingleton(MockTerminalOptions.NewLegacyOptions());

            opt2.AddSingleton<ITerminalExceptionHandler>(new MockExceptionPublisher());
            opt2.AddSingleton<ITerminalRouter<TerminalTcpRouterContext>, TerminalTcpRouter>();
            opt2.AddSingleton(textHandler);
            opt2.AddSingleton<ITerminalConsole, TerminalSystemConsole>();
        }

        private CancellationTokenSource commandTokenSource = null!;
        private IHost host = null!;
        private IPEndPoint serverIpEndPoint = null!;
        private TerminalStartContext startContext = null!;
        private CancellationTokenSource terminalTokenSource = null!;
        private ITerminalTextHandler textHandler = null!;
    }
}
