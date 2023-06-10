/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Net.Sockets;
using System.Threading;

namespace PerpetualIntelligence.Terminal.Commands.Routers
{
    /// <summary>
    /// The TCP connection data.
    /// </summary>
    public sealed class TcpConnectionData
    {
        /// <summary>
        /// The TCP command server.
        /// </summary>
        public TcpListener Server { get; }

        /// <summary>
        /// The TCP command client.
        /// </summary>
        public TcpClient Client { get; }

        /// <summary>
        /// The cancellation token.
        /// </summary>
        public CancellationToken CancellationToken { get; }

        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="server">The TCP command server.</param>
        /// <param name="client">The TCP command client.</param>
        /// <param name="cancellationToken">The optional cancellation token.</param>
        public TcpConnectionData(TcpListener server, TcpClient client, CancellationToken cancellationToken)
        {
            Server = server;
            Client = client;
            CancellationToken = cancellationToken;
        }
    }
}