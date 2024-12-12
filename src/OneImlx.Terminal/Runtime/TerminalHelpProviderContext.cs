/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using OneImlx.Terminal.Commands;

namespace OneImlx.Terminal.Runtime
{
    /// <summary>
    /// The <see cref="ITerminalHelpProvider"/> context.
    /// </summary>
    public sealed class TerminalHelpProviderContext
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="command">The command descriptor.</param>
        public TerminalHelpProviderContext(Command command)
        {
            Command = command;
        }

        /// <summary>
        /// The command descriptor.
        /// </summary>
        public Command Command { get; set; }
    }
}
