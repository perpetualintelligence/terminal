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
        public async Task SendBatch_SendsBatchSuccessfully()
        {
            using (var tcpListener = new TcpListener(IPAddress.Parse(localHost), port))
            {
                tcpListener.Start();

                var serverTask = Task.Run(async () =>
                {
                    using (var serverClient = await tcpListener.AcceptTcpClientAsync())
                    using (var networkStream = serverClient.GetStream())
                    {
                        byte[] buffer = new byte[1024];
                        int bytesRead = await networkStream.ReadAsync(buffer);
                        string receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        receivedMessage.Should().Be("{\"batch_id\":\"batch1\",\"requests\":[{\"id\":\"id1\",\"is_error\":false,\"raw\":\"cmd1\",\"result\":null},{\"id\":\"id2\",\"is_error\":false,\"raw\":\"cmd2\",\"result\":null}]}\u001e");
                    }
                });

                using (var tcpClient = new TcpClient())
                {
                    await tcpClient.ConnectAsync(localHost, port);

                    TerminalInput batch = TerminalInput.Batch("batch1", ["id1", "id2"], ["cmd1", "cmd2"]);
                    await tcpClient.SendToTerminalAsync(batch, streamDelimiter, CancellationToken.None);

                    await serverTask;
                }
            }
        }

        [Fact]
        public async Task SendSingle_AsBatch_SendsSingleSuccessfully()
        {
            using (var tcpListener = new TcpListener(IPAddress.Parse(localHost), port))
            {
                tcpListener.Start();

                var serverTask = Task.Run(async () =>
                {
                    using (var serverClient = await tcpListener.AcceptTcpClientAsync())
                    using (var networkStream = serverClient.GetStream())
                    {
                        byte[] buffer = new byte[1024];
                        int bytesRead = await networkStream.ReadAsync(buffer);
                        string receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        receivedMessage.Should().Be("{\"batch_id\":\"bid\",\"requests\":[{\"id\":\"single-id\",\"is_error\":false,\"raw\":\"single-command\",\"result\":null}]}\u001e");
                    }
                });

                using (var tcpClient = new TcpClient())
                {
                    await tcpClient.ConnectAsync(localHost, port);
                    TerminalInput batch = TerminalInput.Batch("bid", ["single-id"], ["single-command"]);
                    await tcpClient.SendToTerminalAsync(batch, streamDelimiter, CancellationToken.None);

                    await serverTask;
                }
            }
        }

        [Fact]
        public async Task SendSingle_SendsSuccessfully()
        {
            using (var tcpListener = new TcpListener(IPAddress.Parse(localHost), port))
            {
                tcpListener.Start();

                var serverTask = Task.Run(async () =>
                {
                    using (var serverClient = await tcpListener.AcceptTcpClientAsync())
                    using (var networkStream = serverClient.GetStream())
                    {
                        byte[] buffer = new byte[1024];
                        int bytesRead = await networkStream.ReadAsync(buffer);
                        string receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        receivedMessage.Should().Be("{\"batch_id\":null,\"requests\":[{\"id\":\"single-id-1\",\"is_error\":false,\"raw\":\"single-command-1\",\"result\":null}]}\u001e");
                    }
                });

                using (var tcpClient = new TcpClient())
                {
                    await tcpClient.ConnectAsync(localHost, port);
                    TerminalInput single = TerminalInput.Single("single-id-1", "single-command-1");
                    await tcpClient.SendToTerminalAsync(single, streamDelimiter, CancellationToken.None);

                    await serverTask;
                }
            }
        }

        [Fact]
        public async Task SendSingle_ThrowsWhenNotConnected()
        {
            using (var tcpClient = new TcpClient())
            {
                TerminalInput single = TerminalInput.Single("single-id", "single-command");
                Func<Task> act = async () => await tcpClient.SendToTerminalAsync(single, streamDelimiter, CancellationToken.None);

                await act.Should().ThrowAsync<TerminalException>()
                    .WithMessage("The TCP client is not connected.")
                    .WithErrorCode("connection_closed");
            }
        }

        private const string localHost = "127.0.0.1";
        private const int port = 9000;
        private byte streamDelimiter = TerminalIdentifiers.StreamDelimiter;
    }
}
