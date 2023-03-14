/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using System.Threading;

namespace PerpetualIntelligence.Cli.Runtime
{
    /// <summary>
    /// An abstraction of runtime context while starting the terminal.
    /// </summary>
    public interface ITerminalStartContext
    {
        /// <summary>
        /// The command line options.
        /// </summary>
        string[]? Arguments { get; }

        /// <summary>
        /// The cancellation token.
        /// </summary>
        public CancellationTokenSource? CancellationTokenSource { get; }

        /// <summary>
        /// The terminal start information.
        /// </summary>
        public TerminalStartInfo StartInformation { get; }
    }
}