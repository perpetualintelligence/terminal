/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

namespace PerpetualIntelligence.Cli.Commands.Providers
{
    /// <summary>
    /// The argument default value provider context.
    /// </summary>
    public class ArgumentDefaultValueProviderContext
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="commandDescriptor">The command descriptor.</param>
        public ArgumentDefaultValueProviderContext(CommandDescriptor commandDescriptor)
        {
            CommandDescriptor = commandDescriptor;
        }

        /// <summary>
        /// The command descriptor.
        /// </summary>
        public CommandDescriptor CommandDescriptor { get; set; }
    }
}
