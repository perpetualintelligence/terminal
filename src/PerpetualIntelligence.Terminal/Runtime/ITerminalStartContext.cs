/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Threading;

namespace PerpetualIntelligence.Terminal.Runtime
{
    /// <summary>
    /// An abstraction of runtime context while starting the terminal.
    /// </summary>
    public interface ITerminalStartContext
    {
        /// <summary>
        /// The terminal driver arguments or the command line arguments.
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