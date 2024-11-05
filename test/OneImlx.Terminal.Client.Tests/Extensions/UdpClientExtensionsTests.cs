/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace OneImlx.Terminal.Client.Extensions
{
    [Collection("Sequential")]
    public class UdpClientExtensionsTests
    {
        [Fact]
        public async Task SendBatchToTerminalAsync_Sends_Batch_Commands_Successfully()
        {
            var serverReady = new TaskCompletionSource<bool>();

            // Arrange: Start a UdpClient (server) to receive messages
            var serverTask = Task.Run(async () =>
            {
                using (var udpServer = new UdpClient(port))
                {
                    serverReady.SetResult(true);
                    UdpReceiveResult result = await udpServer.ReceiveAsync();
                    string receivedMessage = Encoding.UTF8.GetString(result.Buffer);

                    // Assert: Verify the server received the correct message
                    receivedMessage.Should().Be("command1;command2|");
                }
            });

            // Act: Create a UdpClient (client) to send messages
            using (var udpClient = new UdpClient())
            {
                await serverReady.Task;
                var remoteEndPoint = new IPEndPoint(IPAddress.Parse(localHost), port);
                string[] commands = { "command1", "command2" };
                await udpClient.SendBatchAsync(commands, ";", "|", Encoding.UTF8, remoteEndPoint, CancellationToken.None);

                // Wait for the server to receive the message
                await serverTask;
            }
        }

        [Fact]
        public async Task SendSingleToTerminalAsync_Sends_Single_Command_With_Delimiters_Successfully()
        {
            var serverReady = new TaskCompletionSource<bool>();

            // Arrange: Start a UdpClient (server) to receive messages
            var serverTask = Task.Run(async () =>
            {
                using (var udpServer = new UdpClient(port))
                {
                    serverReady.SetResult(true);
                    UdpReceiveResult result = await udpServer.ReceiveAsync();
                    string receivedMessage = Encoding.UTF8.GetString(result.Buffer);

                    // Assert: Verify the server received the correct message
                    receivedMessage.Should().Be("single-command|");
                }
            });

            // Act: Create a UdpClient (client)
            using (var udpClient = new UdpClient())
            {
                await serverReady.Task;
                var remoteEndPoint = new IPEndPoint(IPAddress.Parse(localHost), port);
                string command = "single-command";
                await udpClient.SendSingleAsync(command, ";", "|", Encoding.UTF8, remoteEndPoint, CancellationToken.None);

                // Wait for the server to receive the message
                await serverTask;
            }
        }

        [Fact]
        public async Task SendSingleToTerminalAsync_Sends_Single_Command_Without_Delimiters_Successfully()
        {
            var serverReady = new TaskCompletionSource<bool>();

            // Arrange: Start a UdpClient (server) to receive messages
            var serverTask = Task.Run(async () =>
            {
                using (var udpServer = new UdpClient(port))
                {
                    serverReady.SetResult(true);
                    UdpReceiveResult result = await udpServer.ReceiveAsync();
                    string receivedMessage = Encoding.UTF8.GetString(result.Buffer);

                    // Assert: Verify the server received the correct message
                    receivedMessage.Should().Be("single-command");
                }
            });

            // Act: Create a UdpClient (client)
            using (var udpClient = new UdpClient())
            {
                await serverReady.Task;
                var remoteEndPoint = new IPEndPoint(IPAddress.Parse(localHost), port);
                string command = "single-command";
                await udpClient.SendSingleAsync(command, Encoding.UTF8, remoteEndPoint, CancellationToken.None);

                // Wait for the server to receive the message
                await serverTask;
            }
        }

        [Fact]
        public async Task SendSingleToTerminalAsync_Throws_Exception_When_UdpClient_Is_Closed()
        {
            // Arrange: Create a UdpClient and close it before sending
            using (var udpClient = new UdpClient())
            {
                udpClient.Close();

                // Act
                Func<Task> act = async () => await udpClient.SendSingleAsync("test-command", Encoding.UTF8, new IPEndPoint(IPAddress.Parse(localHost), port), CancellationToken.None);

                // Assert: Expect an ObjectDisposedException because the UdpClient is closed
                await act.Should().ThrowAsync<ObjectDisposedException>();
            }
        }

        private const string localHost = "127.0.0.1";
        private const int port = 9001;
    }
}
