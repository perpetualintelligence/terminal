/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

namespace OneImlx.Terminal.Runtime
{
    /// <summary>
    /// The terminal start mode.
    /// </summary>
    public enum TerminalStartMode
    {
        /// <summary>
        /// The terminal starts as a TCP server that listens to an incoming connection.
        /// </summary>
        Tcp = 1,

        /// <summary>
        /// The terminal starts as a HTTP server that listens to an incoming message.
        /// </summary>
        Http = 2,

        /// <summary>
        /// The terminal starts as a gRPC server that listens to an incoming message.
        /// </summary>
        Grpc = 3,

        /// <summary>
        /// The terminal starts as a UDP server that listens to an incoming message.
        /// </summary>
        Udp = 4,

        /// <summary>
        /// The terminal starts as a console server or a console application and listens to an incoming user message.
        /// </summary>
        Console = 100,

        /// <summary>
        /// The terminal starts as a custom service.
        /// </summary>
        Custom = 0
    }
}
