/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Threading;

namespace PerpetualIntelligence.Terminal.Runtime
{
    /// <summary>
    /// The terminal start context.
    /// </summary>
    public sealed class TerminalStartContext
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="startInformation">The terminal start information.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="options">The start or command line options.</param>
        public TerminalStartContext(TerminalStartInfo startInformation, CancellationToken cancellationToken, string[]? options = null)
        {
            StartInformation = startInformation;
            CancellationToken = cancellationToken;
            Arguments = options;
        }

        /// <summary>
        /// The terminal driver arguments or command line arguments.
        /// </summary>
        public string[]? Arguments { get; }

        /// <summary>
        /// The cancellation token source.
        /// </summary>
        public CancellationToken CancellationToken { get; }

        /// <summary>
        /// The terminal start information.
        /// </summary>
        public TerminalStartInfo StartInformation { get; }
    }
}