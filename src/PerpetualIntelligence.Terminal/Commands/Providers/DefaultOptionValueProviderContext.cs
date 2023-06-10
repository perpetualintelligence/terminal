/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

namespace PerpetualIntelligence.Terminal.Commands.Providers
{
    /// <summary>
    /// The option default value provider context.
    /// </summary>
    public class DefaultOptionValueProviderContext
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="commandDescriptor">The command descriptor.</param>
        public DefaultOptionValueProviderContext(CommandDescriptor commandDescriptor)
        {
            CommandDescriptor = commandDescriptor;
        }

        /// <summary>
        /// The command descriptor.
        /// </summary>
        public CommandDescriptor CommandDescriptor { get; set; }
    }
}
