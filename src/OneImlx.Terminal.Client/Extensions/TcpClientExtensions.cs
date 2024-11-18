/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using OneImlx.Terminal.Runtime;

namespace OneImlx.Terminal.Client.Extensions
{
    /// <summary>
    /// Provides extension methods for the <see cref="TcpClient"/> class to interact with terminal servers via TCP.
    /// </summary>
    public static class TcpClientExtensions
    {
        /// <summary>
        /// Sends a batch of terminal commands to a server via TCP.
        /// </summary>
        /// <param name="tcpClient">The <see cref="TcpClient"/> instance used to send the request.</param>
        /// <param name="batch">The <see cref="TerminalBatch"/> to send.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while awaiting completion.</param>
        /// <param name="serializeOptions">Optional serialization options for JSON encoding.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public static async Task SendBatchAsync(this TcpClient tcpClient, TerminalBatch batch, CancellationToken cancellationToken, JsonSerializerOptions? serializeOptions = null)
        {
            if (batch == null)
            {
                throw new ArgumentNullException(nameof(batch), "The batch cannot be null.");
            }

            await SendRawToTerminalAsync(tcpClient, batch, cancellationToken, serializeOptions);
        }

        /// <summary>
        /// Sends a single terminal command request to a server via TCP.
        /// </summary>
        /// <param name="tcpClient">The <see cref="TcpClient"/> instance used to send the request.</param>
        /// <param name="command">The <see cref="TerminalRequest"/> command to send.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while awaiting completion.</param>
        /// <param name="serializeOptions">Optional serialization options for JSON encoding.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public static async Task SendSingleAsync(this TcpClient tcpClient, TerminalRequest command, CancellationToken cancellationToken, JsonSerializerOptions? serializeOptions = null)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command), "The command cannot be null.");
            }

            await SendRawToTerminalAsync(tcpClient, command, cancellationToken, serializeOptions);
        }

        private static async Task SendRawToTerminalAsync<TInput>(TcpClient tcpClient, TInput raw, CancellationToken cancellationToken, JsonSerializerOptions? serializeOptions = null)
        {
            if (tcpClient == null)
            {
                throw new ArgumentNullException(nameof(tcpClient), "The TCP client cannot be null.");
            }

            if (!tcpClient.Connected)
            {
                throw new TerminalException(TerminalErrors.ConnectionClosed, "The TCP client is not connected.");
            }

            serializeOptions ??= JsonSerializerOptions.Default;
            byte[] messageBytes = JsonSerializer.SerializeToUtf8Bytes(raw, serializeOptions);
            var networkStream = tcpClient.GetStream();

            await networkStream.WriteAsync(messageBytes, 0, messageBytes.Length, cancellationToken).ConfigureAwait(false);
            await networkStream.FlushAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
