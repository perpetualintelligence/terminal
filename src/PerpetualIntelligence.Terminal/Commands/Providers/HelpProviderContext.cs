/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

namespace PerpetualIntelligence.Terminal.Commands.Providers
{
    /// <summary>
    /// The <see cref="IHelpProvider"/> context.
    /// </summary>
    public sealed class HelpProviderContext
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="command">The command descriptor.</param>
        public HelpProviderContext(Command command)
        {
            Command = command;
        }

        /// <summary>
        /// The command descriptor.
        /// </summary>
        public Command Command { get; set; }
    }
}