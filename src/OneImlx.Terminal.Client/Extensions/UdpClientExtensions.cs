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
using OneImlx.Terminal.Runtime;

namespace OneImlx.Terminal.Client.Extensions
{
    /// <summary>
    /// Provides extension methods for the <see cref="UdpClient"/> class to interact with terminal servers via UDP.
    /// </summary>
    public static class UdpClientExtensions
    {
        /// <summary>
        /// Sends multiple command strings to a terminal server in a single batch UDP message using the specified encoding.
        /// </summary>
        /// <param name="udpClient">The <see cref="UdpClient"/> instance used to send the request.</param>
        /// <param name="commands">An array of command strings to send as a batch.</param>
        /// <param name="cmdDelimiter">The delimiter used to separate commands.</param>
        /// <param name="msgDelimiter">The delimiter used to separate parts of the message.</param>
        /// <param name="encoding">The <see cref="Encoding"/> used to encode the message before sending.</param>
        /// <param name="remoteEndPoint">The remote server endpoint to send the commands to.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while awaiting completion.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public static async Task SendBatchToTerminalAsync(this UdpClient udpClient, string[] commands, string cmdDelimiter, string msgDelimiter, Encoding encoding, IPEndPoint remoteEndPoint, CancellationToken cancellationToken)
        {
            string batchCommands = TerminalServices.DelimitedMessage(cmdDelimiter, msgDelimiter, commands);
            await SendMessageToUdpAsync(udpClient, batchCommands, encoding, remoteEndPoint, cancellationToken);
        }

        /// <summary>
        /// Sends a single command string to a terminal server via a UDP message using the specified encoding and delimiters.
        /// </summary>
        /// <param name="udpClient">The <see cref="UdpClient"/> instance used to send the request.</param>
        /// <param name="commandString">The command string to send.</param>
        /// <param name="cmdDelimiter">The delimiter used to separate commands.</param>
        /// <param name="msgDelimiter">The delimiter used to separate parts of the message.</param>
        /// <param name="encoding">The <see cref="Encoding"/> used to encode the message before sending.</param>
        /// <param name="remoteEndPoint">The remote server endpoint to send the command to.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while awaiting completion.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public static async Task SendSingleToTerminalAsync(this UdpClient udpClient, string commandString, string cmdDelimiter, string msgDelimiter, Encoding encoding, IPEndPoint remoteEndPoint, CancellationToken cancellationToken)
        {
            string delimitedCommand = TerminalServices.DelimitedMessage(cmdDelimiter, msgDelimiter, commandString);
            await SendMessageToUdpAsync(udpClient, delimitedCommand, encoding, remoteEndPoint, cancellationToken);
        }

        /// <summary>
        /// Sends a single command string to a terminal server via a UDP message using the specified encoding, without delimiters.
        /// </summary>
        /// <param name="udpClient">The <see cref="UdpClient"/> instance used to send the request.</param>
        /// <param name="commandString">The command string to send.</param>
        /// <param name="encoding">The <see cref="Encoding"/> used to encode the message before sending.</param>
        /// <param name="remoteEndPoint">The remote server endpoint to send the command to.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while awaiting completion.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public static async Task SendSingleToTerminalAsync(this UdpClient udpClient, string commandString, Encoding encoding, IPEndPoint remoteEndPoint, CancellationToken cancellationToken)
        {
            await SendMessageToUdpAsync(udpClient, commandString, encoding, remoteEndPoint, cancellationToken);
        }

        /// <summary>
        /// Helper method to send a message asynchronously to a UDP server using <see cref="UdpClient"/> with the
        /// specified encoding.
        /// </summary>
        /// <param name="udpClient">The <see cref="UdpClient"/> instance used to send the request.</param>
        /// <param name="message">The message to send.</param>
        /// <param name="encoding">The <see cref="Encoding"/> used to encode the message before sending.</param>
        /// <param name="remoteEndPoint">The remote server endpoint to send the message to.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while awaiting completion.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        private static async Task SendMessageToUdpAsync(UdpClient udpClient, string message, Encoding encoding, IPEndPoint remoteEndPoint, CancellationToken cancellationToken)
        {
            // Send the message asynchronously
            byte[] messageBytes = encoding.GetBytes(message);
            await udpClient.SendAsync(messageBytes, messageBytes.Length, remoteEndPoint);
        }
    }
}
