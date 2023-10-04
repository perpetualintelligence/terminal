/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Collections.Generic;

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
        /// <param name="customProperties"></param>
        public TerminalStartInfo(TerminalStartMode startMode, Dictionary<string, object>? customProperties = null)
        {
            StartMode = startMode;
            CustomProperties = customProperties;
        }

        /// <summary>
        /// The custom properties.
        /// </summary>
        public Dictionary<string, object>? CustomProperties { get; }

        /// <summary>
        /// The terminal start mode.
        /// </summary>
        public TerminalStartMode StartMode { get; }
    }
}