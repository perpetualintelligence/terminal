/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Threading;

namespace PerpetualIntelligence.Terminal.Runtime
{
    /// <summary>
    /// The default <see cref="ITerminalStartContext"/>.
    /// </summary>
    public sealed class TerminalStartContext : ITerminalStartContext
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="startInformation">The terminal start information.</param>
        /// <param name="cancellationTokenSource">The cancellation token.</param>
        /// <param name="options">The start or command line options.</param>
        public TerminalStartContext(TerminalStartInfo startInformation, CancellationTokenSource cancellationTokenSource, string[]? options = null)
        {
            StartInformation = startInformation;
            CancellationTokenSource = cancellationTokenSource;
            Arguments = options;
        }

        /// <summary>
        /// The terminal driver arguments or command line arguments.
        /// </summary>
        public string[]? Arguments { get; }

        /// <summary>
        /// The cancellation token source.
        /// </summary>
        public CancellationTokenSource CancellationTokenSource { get; }

        /// <summary>
        /// The terminal start information.
        /// </summary>
        public TerminalStartInfo StartInformation { get; }
    }
}