/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

namespace PerpetualIntelligence.Cli.Commands.Providers
{
    /// <summary>
    /// The argument default provider context.
    /// </summary>
    public class DefaultArgumentProviderContext
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="commandDescriptor">The command descriptor.</param>
        public DefaultArgumentProviderContext(CommandDescriptor commandDescriptor)
        {
            CommandDescriptor = commandDescriptor;
        }

        /// <summary>
        /// The command descriptor.
        /// </summary>
        public CommandDescriptor CommandDescriptor { get; set; }
    }
}
