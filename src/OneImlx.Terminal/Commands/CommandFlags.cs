﻿/*
    Copyright 2024 (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;

namespace OneImlx.Terminal.Commands
{
    /// <summary>
    /// Defines special command flags.
    /// </summary>
    [Flags]
    public enum CommandFlags
    {
        /// <summary>
        /// No special flags.
        /// </summary>
        None = 0,

        /// <summary>
        /// The command requires an authorization.
        /// </summary>
        Authorize = 2,

        /// <summary>
        /// The command is obsolete.
        /// </summary>
        Obsolete = 4,
    }
}
