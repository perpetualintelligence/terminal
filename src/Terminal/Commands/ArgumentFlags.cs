/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;

namespace PerpetualIntelligence.Terminal.Commands
{
    /// <summary>
    /// Defines special argument flags.
    /// </summary>
    [Flags]
    public enum ArgumentFlags
    {
        /// <summary>
        /// No special flags.
        /// </summary>
        None = 0,

        /// <summary>
        /// The argument is required.
        /// </summary>
        Required = 2,

        /// <summary>
        /// The argument is obsolete.
        /// </summary>
        Obsolete = 4,

        /// <summary>
        /// The argument is disabled.
        /// </summary>
        Disabled = 8,
    }
}