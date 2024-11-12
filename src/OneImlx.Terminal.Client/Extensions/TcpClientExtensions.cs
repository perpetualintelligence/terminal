/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Net.Sockets;
using System.Text;
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
        /// Sends multiple command strings to a terminal server in a single batch TCP message using the specified encoding.
        /// </summary>
        /// <param name="tcpClient">The <see cref="TcpClient"/> instance used to send the request.</param>
        /// <param name="commands">An array of command strings to send as a batch.</param>
        /// <param name="cmdDelimiter">The delimiter used to separate commands.</param>
        /// <param name="msgDelimiter">The delimiter used to separate parts of the message.</param>
        /// <param name="encoding">The <see cref="Encoding"/> used to encode the message before sending.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while awaiting completion.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public static async Task SendBatchAsync(this TcpClient tcpClient, string[] commands, string cmdDelimiter, string msgDelimiter, Encoding encoding, CancellationToken cancellationToken)
        {
            string batch = TerminalServices.CreateBatch(cmdDelimiter, msgDelimiter, commands);
            await SendMessageToTerminalAsync(tcpClient, batch, encoding, cancellationToken);
        }

        /// <summary>
        /// Sends a single command string to a terminal server via a TCP message using the specified encoding and delimiters.
        /// </summary>
        /// <param name="tcpClient">The <see cref="TcpClient"/> instance used to send the request.</param>
        /// <param name="raw">The command string to send.</param>
        /// <param name="cmdDelimiter">The delimiter used to separate commands.</param>
        /// <param name="msgDelimiter">The delimiter used to separate parts of the message.</param>
        /// <param name="encoding">The <see cref="Encoding"/> used to encode the message before sending.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while awaiting completion.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public static async Task SendSingleAsync(this TcpClient tcpClient, string raw, string cmdDelimiter, string msgDelimiter, Encoding encoding, CancellationToken cancellationToken)
        {
            string batchCommand = TerminalServices.CreateBatch(cmdDelimiter, msgDelimiter, [raw]);
            await SendMessageToTerminalAsync(tcpClient, batchCommand, encoding, cancellationToken);
        }

        /// <summary>
        /// Sends a single command string to a terminal server via a TCP message using the specified encoding, without delimiters.
        /// </summary>
        /// <param name="tcpClient">The <see cref="TcpClient"/> instance used to send the request.</param>
        /// <param name="commandString">The command string to send.</param>
        /// <param name="encoding">The <see cref="Encoding"/> used to encode the message before sending.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while awaiting completion.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public static async Task SendSingleAsync(this TcpClient tcpClient, string commandString, Encoding encoding, CancellationToken cancellationToken)
        {
            await SendMessageToTerminalAsync(tcpClient, commandString, encoding, cancellationToken);
        }

        /// <summary>
        /// Helper method to send a message asynchronously to a terminal server using <see cref="TcpClient"/> with the
        /// specified encoding.
        /// </summary>
        /// <param name="tcpClient">The <see cref="TcpClient"/> instance used to send the request.</param>
        /// <param name="message">The message to send.</param>
        /// <param name="encoding">The <see cref="Encoding"/> used to encode the message before sending.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while awaiting completion.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        private static async Task SendMessageToTerminalAsync(TcpClient tcpClient, string message, Encoding encoding, CancellationToken cancellationToken)
        {
            // Ensure the TCP client is connected
            if (!tcpClient.Connected)
            {
                throw new TerminalException(TerminalErrors.ConnectionClosed, "The TCP client is not connected.");
            }

            // Write the message to the NetworkStream
            byte[] messageBytes = encoding.GetBytes(message);
            await tcpClient.GetStream().WriteAsync(messageBytes, 0, messageBytes.Length, cancellationToken);
            await tcpClient.GetStream().FlushAsync();
        }
    }
}
