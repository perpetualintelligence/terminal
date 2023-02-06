/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

namespace PerpetualIntelligence.Cli.Runtime
{
    /// <summary>
    /// The terminal start mode.
    /// </summary>
    public enum TerminalStartMode
    {
        /// <summary>
        /// The terminal starts as a server that listens to an incoming connection.
        /// </summary>
        Server = 0,

        /// <summary>
        /// The terminal starts as a  console application.
        /// </summary>
        Console = 1,

        /// <summary>
        /// The terminal starts as a custom service.
        /// </summary>
        Custom = 2
    }
}