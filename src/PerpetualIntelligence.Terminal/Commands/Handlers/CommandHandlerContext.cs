/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Terminal.Licensing;
using System;

namespace PerpetualIntelligence.Terminal.Commands.Handlers
{
    /// <summary>
    /// The command handler context.
    /// </summary>
    public sealed class CommandHandlerContext
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="command">The command handle.</param>
        /// <param name="license">The extracted license.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public CommandHandlerContext(Command command, License license)
        {
            Command = command ?? throw new ArgumentNullException(nameof(command));
            License = license ?? throw new ArgumentNullException(nameof(license));
        }

        /// <summary>
        /// The command to handle.
        /// </summary>
        public Command Command { get; }

        /// <summary>
        /// The extracted licenses.
        /// </summary>
        public License License { get; }
    }
}