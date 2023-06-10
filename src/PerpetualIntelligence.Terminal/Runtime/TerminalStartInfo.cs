/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

namespace PerpetualIntelligence.Terminal.Runtime
{
    /// <summary>
    /// The terminal start information.
    /// </summary>
    public sealed class TerminalStartInfo
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="startMode">The terminal start mode.</param>
        /// <param name="port">The optional port number of the endpoint, if the <see cref="TerminalStartMode"/> is <see cref="TerminalStartMode.Server"/>.</param>
        public TerminalStartInfo(TerminalStartMode startMode, int? port = null)
        {
            Port = port;
            StartMode = startMode;
        }

        /// <summary>
        /// The optional port number of the endpoint.
        /// </summary>
        public int? Port { get; }

        /// <summary>
        /// The terminal start mode.
        /// </summary>
        public TerminalStartMode StartMode { get; }
    }
}