/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

namespace OneImlx.Terminal.Commands
{
    /// <summary>
    /// Defines the command type.
    /// </summary>
    public enum CommandType
    {
        /// <summary>
        /// The command represents the root command.
        /// </summary>
        RootCommand = 1,

        /// <summary>
        /// The command represents a group of sub-commands within a root.
        /// </summary>
        GroupCommand = 2,

        /// <summary>
        /// The command represents a sub-command within a group.
        /// </summary>
        SubCommand = 3,

        /// <summary>
        /// The command represents a native command to the terminal. For example <c>cls</c> that clears the terminal, or
        /// <c>run</c> command that executes a native OS command.
        /// </summary>
        NativeCommand = 4,
    }
}
