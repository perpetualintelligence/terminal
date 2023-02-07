/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using System.Threading;

namespace PerpetualIntelligence.Cli.Runtime
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
        /// <param name="arguments">The start or command line arguments.</param>
        public TerminalStartContext(TerminalStartInfo startInformation, CancellationTokenSource? cancellationTokenSource, string[]? arguments)
        {
            StartInformation = startInformation;
            CancellationTokenSource = cancellationTokenSource;
            Arguments = arguments;
        }

        /// <summary>
        /// The command line arguments.
        /// </summary>
        public string[]? Arguments { get; set; }

        /// <summary>
        /// The cancellation token source.
        /// </summary>
        public CancellationTokenSource? CancellationTokenSource { get; set; }

        /// <summary>
        /// The terminal start information.
        /// </summary>
        public TerminalStartInfo StartInformation { get; set; } = null!;
    }
}