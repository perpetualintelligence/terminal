/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

namespace PerpetualIntelligence.Cli.Configuration.Runtime
{
    /// <summary>
    /// The terminal start mode.
    /// </summary>
    public enum TerminalStartMode
    {
        /// <summary>
        /// The terminal starts as a service that listens to an incoming connection.
        /// </summary>
        Service = 0,

        /// <summary>
        /// The terminal starts as a  console application.
        /// </summary>
        Console = 1
    }
}