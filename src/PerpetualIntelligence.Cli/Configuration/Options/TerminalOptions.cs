﻿/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

namespace PerpetualIntelligence.Cli.Configuration.Options
{
    /// <summary>
    /// The terminal configuration options.
    /// </summary>
    public class TerminalOptions
    {
        /// <summary>
        /// The indent size for the terminal logger messages.
        /// </summary>
        public int LoggerIndent { get; set; } = 4;
    }
}