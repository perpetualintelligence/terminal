﻿/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

namespace PerpetualIntelligence.Cli.Commands.Providers
{
    /// <summary>
    /// The option default provider context.
    /// </summary>
    public class DefaultOptionProviderContext
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="commandDescriptor">The command descriptor.</param>
        public DefaultOptionProviderContext(CommandDescriptor commandDescriptor)
        {
            CommandDescriptor = commandDescriptor;
        }

        /// <summary>
        /// The command descriptor.
        /// </summary>
        public CommandDescriptor CommandDescriptor { get; set; }
    }
}