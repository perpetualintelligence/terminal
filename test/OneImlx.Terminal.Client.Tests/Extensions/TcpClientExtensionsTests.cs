/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using OneImlx.Terminal.Runtime;
using OneImlx.Test.FluentAssertions;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace OneImlx.Terminal.Client.Extensions.Tests
{
    public class TcpClientExtensionsTests
    {
        [Fact]
        public async Task SendBatchToTerminalAsync_Sends_Batch_Commands_Successfully()
        {
            // Arrange: Start the TcpListener (server) to accept connections
            using (var tcpListener = new TcpListener(IPAddress.Parse(localHost), port))
            {
                tcpListener.Start();

                // Simulate server-side listener in a task
                var serverTask = Task.Run(async () =>
                {
                    using (var serverClient = await tcpListener.AcceptTcpClientAsync())
                    using (var networkStream = serverClient.GetStream())
                    {
                        byte[] buffer = new byte[1024];
                        int bytesRead = await networkStream.ReadAsync(buffer);
                        string receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                        // Assert: Verify the server received the correct message
                        receivedMessage.Should().Be("{\"batch_id\":\"batch-id\",\"requests\":[{\"id\":\"cmd_id_1\",\"raw\":\"command1\"},{\"id\":\"cmd_id_2\",\"raw\":\"command2\"}]}");
                    }
                });

                // Act: Create a TcpClient (client) to connect to the server
                using (var tcpClient = new TcpClient())
                {
                    await tcpClient.ConnectAsync(localHost, port);

                    // Send a batch of commands
                    TerminalBatch batch = new("batch-id")
                    {
                        { "cmd_id_1", "command1" },
                        { "cmd_id_2", "command2" }
                    };
                    await tcpClient.SendBatchAsync(batch, CancellationToken.None);

                    // Wait for the server to complete
                    await serverTask;
                }
            }
        }

        [Fact]
        public async Task SendSingle_AsBatch_ToTerminalAsync_Sends_Single_Command_Successfully()
        {
            // Arrange: Start the TcpListener (server)
            using (var tcpListener = new TcpListener(IPAddress.Parse(localHost), port))
            {
                tcpListener.Start();

                // Simulate server-side listener in a task
                var serverTask = Task.Run(async () =>
                {
                    using (var serverClient = await tcpListener.AcceptTcpClientAsync())
                    using (var networkStream = serverClient.GetStream())
                    {
                        byte[] buffer = new byte[1024];
                        int bytesRead = await networkStream.ReadAsync(buffer);
                        string receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                        // Assert: Verify the server received the correct message
                        receivedMessage.Should().Be("{\"batch_id\":\"bid\",\"requests\":[{\"id\":\"single-id\",\"raw\":\"single-command\"}]}");
                    }
                });

                // Act: Create a TcpClient (client)
                using (var tcpClient = new TcpClient())
                {
                    await tcpClient.ConnectAsync(localHost, port);
                    TerminalBatch batch = new("bid")
                    {
                        { "single-id", "single-command" }
                    };
                    await tcpClient.SendBatchAsync(batch, CancellationToken.None);

                    // Wait for the server to complete
                    await serverTask;
                }
            }
        }

        [Fact]
        public async Task SendSingleToTerminalAsync_Sends_Single_No_Delimiter_Command_Successfully()
        {
            // Arrange: Start the TcpListener (server)
            using (var tcpListener = new TcpListener(IPAddress.Parse(localHost), port))
            {
                tcpListener.Start();

                // Simulate server-side listener in a task
                var serverTask = Task.Run(async () =>
                {
                    using (var serverClient = await tcpListener.AcceptTcpClientAsync())
                    using (var networkStream = serverClient.GetStream())
                    {
                        byte[] buffer = new byte[1024];
                        int bytesRead = await networkStream.ReadAsync(buffer);
                        string receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                        // Assert: Verify the server received the correct message
                        receivedMessage.Should().Be("{\"id\":\"single-id-1\",\"raw\":\"single-command-1\"}");
                    }
                });

                // Act: Create a TcpClient (client)
                using (var tcpClient = new TcpClient())
                {
                    await tcpClient.ConnectAsync(localHost, port);
                    TerminalRequest single = new("single-id-1", "single-command-1");
                    await tcpClient.SendSingleAsync(single, CancellationToken.None);

                    // Wait for the server to complete
                    await serverTask;
                }
            }
        }

        [Fact]
        public async Task SendSingleToTerminalAsync_Throws_When_TcpClient_Not_Connected()
        {
            // Arrange: Create a TcpClient without connecting
            using (var tcpClient = new TcpClient())
            {
                // Act
                TerminalRequest single = new("single-id", "single-command");
                Func<Task> act = async () => await tcpClient.SendSingleAsync(single, CancellationToken.None);

                // Assert: Expect an InvalidOperationException because the client is not connected
                await act.Should().ThrowAsync<TerminalException>()
                    .WithErrorCode("connection_closed")
                    .WithErrorDescription("The TCP client is not connected.");
            }
        }

        private const string localHost = "127.0.0.1";
        private const int port = 9000;
    }
}
