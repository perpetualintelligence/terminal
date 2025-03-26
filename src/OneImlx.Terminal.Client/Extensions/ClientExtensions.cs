/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using OneImlx.Terminal.Runtime;
using OneImlx.Terminal.Shared;

namespace OneImlx.Terminal.Client.Extensions
{
    /// <summary>
    /// Provides extension methods for interacting with terminal server over various network protocols.
    /// </summary>
    public static class ClientExtensions
    {
        /// <summary>
        /// Sends a <see cref="TerminalInputOutput"/> object to a terminal server as an HTTP POST request.
        /// </summary>
        /// <param name="httpClient">The <see cref="HttpClient"/> instance used to send the request.</param>
        /// <param name="input">The <see cref="TerminalInputOutput"/> to be sent.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while awaiting completion.</param>
        /// <param name="serializeOptions">
        /// The <see cref="JsonSerializerOptions"/> used to serialize the input. Defaults to <c>null</c>.
        /// </param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <remarks>The HTTP POST request is sent to the endpoint <c>oneimlx/terminal/httprouter</c> on the server.</remarks>
        public static async Task<TerminalInputOutput?> SendToTerminalAsync(this HttpClient httpClient, TerminalInputOutput input, CancellationToken cancellationToken, JsonSerializerOptions? serializeOptions = null)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input), "The input cannot be null.");
            }

            serializeOptions ??= JsonSerializerOptions.Default;
            var response = await httpClient.PostAsJsonAsync("oneimlx/terminal/httprouter", input, serializeOptions, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<TerminalInputOutput>(serializeOptions, cancellationToken);
        }

        /// <summary>
        /// Sends a <see cref="TerminalInputOutput"/> object to a terminal server via a UDP message.
        /// </summary>
        /// <param name="udpClient">The <see cref="UdpClient"/> instance used to send the message.</param>
        /// <param name="input">The <see cref="TerminalInputOutput"/> to be sent.</param>
        /// <param name="inputDelimiter">The stream delimiter.</param>
        /// <param name="remoteEndPoint">The <see cref="IPEndPoint"/> representing the remote server endpoint.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while awaiting completion.</param>
        /// <param name="serializeOptions">
        /// The <see cref="JsonSerializerOptions"/> used to serialize the input. Defaults to <c>null</c>.
        /// </param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public static async Task SendToTerminalAsync(this UdpClient udpClient, TerminalInputOutput input, byte inputDelimiter, IPEndPoint remoteEndPoint, CancellationToken cancellationToken, JsonSerializerOptions? serializeOptions = null)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input), "The input cannot be null.");
            }

            serializeOptions ??= JsonSerializerOptions.Default;

            // Create a new array for the message with the delimiter appended
            byte[] messageWithDelimiter = TerminalServices.DelimitBytes(JsonSerializer.SerializeToUtf8Bytes(input, serializeOptions), inputDelimiter);
            await udpClient.SendAsync(messageWithDelimiter, messageWithDelimiter.Length, remoteEndPoint);
        }

        /// <summary>
        /// Sends a <see cref="TerminalInputOutput"/> object to a terminal server via a TCP connection.
        /// </summary>
        /// <param name="tcpClient">The <see cref="TcpClient"/> instance used to send the message.</param>
        /// <param name="input">The <see cref="TerminalInputOutput"/> to be sent.</param>
        /// <param name="inputDelimiter">The stream delimiter.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while awaiting completion.</param>
        /// <param name="serializeOptions">
        /// The <see cref="JsonSerializerOptions"/> used to serialize the input. Defaults to <c>null</c>.
        /// </param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public static async Task SendToTerminalAsync(this TcpClient tcpClient, TerminalInputOutput input, byte inputDelimiter, CancellationToken cancellationToken, JsonSerializerOptions? serializeOptions = null)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input), "The input cannot be null.");
            }

            if (!tcpClient.Connected)
            {
                throw new TerminalException(TerminalErrors.ConnectionClosed, "The TCP client is not connected.");
            }

            serializeOptions ??= JsonSerializerOptions.Default;

            // Create a new array for the message with the delimiter appended
            byte[] messageWithDelimiter = TerminalServices.DelimitBytes(JsonSerializer.SerializeToUtf8Bytes(input, serializeOptions), inputDelimiter);

            // Write the combined message in a single operation
            NetworkStream networkStream = tcpClient.GetStream();
            await networkStream.WriteAsync(messageWithDelimiter, 0, messageWithDelimiter.Length, cancellationToken);
            await networkStream.FlushAsync(cancellationToken);
        }

        /// <summary>
        /// Sends a <see cref="TerminalInputOutput"/> object to a terminal server via a gRPC request.
        /// </summary>
        /// <param name="grpcClient">The gRPC client instance used to send the request.</param>
        /// <param name="input">The <see cref="TerminalInputOutput"/> to be sent.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while awaiting completion.</param>
        /// <param name="serializeOptions">
        /// The <see cref="JsonSerializerOptions"/> used to serialize the input. Defaults to <c>null</c>.
        /// </param>
        /// <returns>
        /// A task representing the asynchronous operation. The task result contains the
        /// <see cref="TerminalGrpcRouterProtoOutput"/> from the server.
        /// </returns>
        public static async Task<TerminalGrpcRouterProtoOutput> SendToTerminalAsync(this TerminalGrpcRouterProto.TerminalGrpcRouterProtoClient grpcClient, TerminalInputOutput input, CancellationToken cancellationToken, JsonSerializerOptions? serializeOptions = null)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input), "The input cannot be null.");
            }

            serializeOptions ??= JsonSerializerOptions.Default;
            var protoInput = new TerminalGrpcRouterProtoInput { InputJson = JsonSerializer.Serialize(input, serializeOptions) };
            return await grpcClient.RouteCommandAsync(protoInput, cancellationToken: cancellationToken);
        }
    }
}
