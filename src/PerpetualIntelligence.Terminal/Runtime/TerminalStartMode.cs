/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

namespace PerpetualIntelligence.Terminal.Runtime
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
        /// The terminal starts as a HTTP server that listens to an incoming message. NOT YET SUPPORTED.
        /// </summary>
        Http = 2,

        /// <summary>
        /// The terminal starts as a gRPC server that listens to an incoming message.  NOT YET SUPPORTED.
        /// </summary>
        Grpc = 3,

        /// <summary>
        /// The terminal starts as a  console application.
        /// </summary>
        Console = 100,

        /// <summary>
        /// The terminal starts as a custom service.
        /// </summary>
        Custom = 0
    }
}