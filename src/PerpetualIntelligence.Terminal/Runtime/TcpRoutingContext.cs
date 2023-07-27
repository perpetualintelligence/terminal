/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Net;
using System.Net.Sockets;

namespace PerpetualIntelligence.Terminal.Runtime
{
    /// <summary>
    /// The TCP connection data.
    /// </summary>
    public sealed class TcpRoutingContext : TerminalRoutingContext
    {
        /// <summary>
        /// The TCP command server.
        /// </summary>
        public TcpListener? Server { get; private set; }

        /// <summary>
        /// The TCP command client.
        /// </summary>
        public TcpClient? Client { get; private set; }

        /// <summary>
        /// The network IP endpoint server will connect. Used for <see cref="TcpRouting"/>.
        /// </summary>
        public IPEndPoint IPEndPoint { get; private set; }

        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="iPEndPoint">The network IP endpoint server will connect.</param>
        /// <param name="terminalStartContext">The terminal start context.</param>
        /// <param name="server">The TCP command server.</param>
        /// <param name="client">The TCP command client.</param>
        public TcpRoutingContext(IPEndPoint iPEndPoint, TerminalStartContext terminalStartContext, TcpListener? server = null, TcpClient? client = null) : base(terminalStartContext)
        {
            IPEndPoint = iPEndPoint;
            Server = server;
            Client = client;
        }

        /// <summary>
        /// Setups the <see cref="Server"/> and <see cref="Client"/>.
        /// </summary>
        /// <param name="tcpServer">The TCP server.</param>
        /// <param name="tcpClient">The TCP client.</param>
        public void Setup(TcpListener tcpServer, TcpClient tcpClient)
        {
            Server = tcpServer;
            Client = tcpClient;
        }
    }
}