/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using OneImlx.Terminal.Runtime;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace OneImlx.Terminal.Client.Extensions.Tests
{
    public class UdpClientExtensionsTests
    {
        [Fact]
        public async Task SendBatch_SendsBatchSuccessfully()
        {
            var serverReady = new TaskCompletionSource<bool>();

            var serverTask = Task.Run(async () =>
            {
                using (var udpServer = new UdpClient(port))
                {
                    serverReady.SetResult(true);
                    UdpReceiveResult result = await udpServer.ReceiveAsync();
                    string receivedMessage = Encoding.UTF8.GetString(result.Buffer);
                    receivedMessage.Should().Be("{\"batch_id\":\"batch1\",\"requests\":[{\"id\":\"id1\",\"is_error\":false,\"raw\":\"cmd1\",\"result\":null},{\"id\":\"id2\",\"is_error\":false,\"raw\":\"cmd2\",\"result\":null}]}\u001e");
                }
            });

            using (var udpClient = new UdpClient())
            {
                await serverReady.Task;
                var remoteEndPoint = new IPEndPoint(IPAddress.Parse(localHost), port);
                TerminalInput batch = TerminalInput.Batch("batch1", ["id1", "id2"], ["cmd1", "cmd2"]);
                await udpClient.SendToTerminalAsync(batch, TerminalIdentifiers.StreamDelimiter, remoteEndPoint, CancellationToken.None);

                await serverTask;
            }
        }

        [Fact]
        public async Task SendSingle_AsBatch_SendsSingleSuccessfully()
        {
            var serverReady = new TaskCompletionSource<bool>();

            var serverTask = Task.Run(async () =>
            {
                using (var udpServer = new UdpClient(port))
                {
                    serverReady.SetResult(true);
                    UdpReceiveResult result = await udpServer.ReceiveAsync();
                    string receivedMessage = Encoding.UTF8.GetString(result.Buffer);
                    receivedMessage.Should().Be("{\"batch_id\":\"bid\",\"requests\":[{\"id\":\"single-id\",\"is_error\":false,\"raw\":\"single-command\",\"result\":null}]}\u001e");
                }
            });

            using (var udpClient = new UdpClient())
            {
                await serverReady.Task;
                var remoteEndPoint = new IPEndPoint(IPAddress.Parse(localHost), port);
                TerminalInput batch = TerminalInput.Batch("bid", ["single-id"], ["single-command"]);
                await udpClient.SendToTerminalAsync(batch, TerminalIdentifiers.StreamDelimiter, remoteEndPoint, CancellationToken.None);

                await serverTask;
            }
        }

        [Fact]
        public async Task SendSingle_SendsSuccessfully()
        {
            var serverReady = new TaskCompletionSource<bool>();

            var serverTask = Task.Run(async () =>
            {
                using (var udpServer = new UdpClient(port))
                {
                    serverReady.SetResult(true);
                    UdpReceiveResult result = await udpServer.ReceiveAsync();
                    string receivedMessage = Encoding.UTF8.GetString(result.Buffer);
                    receivedMessage.Should().Be("{\"batch_id\":null,\"requests\":[{\"id\":\"single-id-1\",\"is_error\":false,\"raw\":\"single-command-1\",\"result\":null}]}\u001e");
                }
            });

            using (var udpClient = new UdpClient())
            {
                await serverReady.Task;
                var remoteEndPoint = new IPEndPoint(IPAddress.Parse(localHost), port);
                TerminalInput single = TerminalInput.Single("single-id-1", "single-command-1");
                await udpClient.SendToTerminalAsync(single, TerminalIdentifiers.StreamDelimiter, remoteEndPoint, CancellationToken.None);

                await serverTask;
            }
        }

        [Fact]
        public async Task SendSingle_ThrowsWhenClientClosed()
        {
            using (var udpClient = new UdpClient())
            {
                udpClient.Close();

                TerminalInput single = TerminalInput.Single("single-id-1", "single-command-1");
                Func<Task> act = async () => await udpClient.SendToTerminalAsync(single, TerminalIdentifiers.StreamDelimiter, new IPEndPoint(IPAddress.Parse(localHost), port), CancellationToken.None);

                await act.Should().ThrowAsync<ObjectDisposedException>();
            }
        }

        private const string localHost = "127.0.0.1";
        private const int port = 9001;
    }
}
