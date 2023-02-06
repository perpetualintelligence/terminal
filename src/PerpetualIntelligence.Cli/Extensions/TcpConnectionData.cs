/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Cli.Configuration.Options;
using System.Net.Sockets;
using System.Threading;

namespace PerpetualIntelligence.Cli.Extensions
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
        /// The  logger.
        /// </summary>
        public ILogger Logger { get; }

        /// <summary>
        /// The services host.
        /// </summary>
        public IHost Host { get; }

        /// <summary>
        /// >The configuration options.
        /// </summary>
        public CliOptions Options { get; }

        /// <summary>
        /// The cancellation token.
        /// </summary>
        public CancellationToken CancellationToken { get; }

        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="server">The TCP command server.</param>
        /// <param name="client">The TCP command client.</param>
        /// <param name="host">The services host.</param>
        /// <param name="options">The configuration options.</param>
        /// <param name="logger">The  logger.</param>
        /// <param name="cancellationToken">The optional cancellation token.</param>
        /// <param name=""></param>
        public TcpConnectionData(TcpListener server, TcpClient client, IHost host, CliOptions options, ILogger logger, CancellationToken cancellationToken)
        {
            Server = server;
            Client = client;
            Host = host;
            Options = options;
            CancellationToken = cancellationToken;
            Logger = logger;
        }
    }
}