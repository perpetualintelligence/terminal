/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

namespace PerpetualIntelligence.Terminal.Commands
{
    /// <summary>
    /// Defines the command type.
    /// </summary>
    public enum CommandType
    {
        /// <summary>
        /// The command represents the root command.
        /// </summary>
        Root = 1,

        /// <summary>
        /// The command represents a group of sub-commands.
        /// </summary>
        Group = 2,

        /// <summary>
        /// The command represents a sub-command.
        /// </summary>
        SubCommand = 3
    }
}