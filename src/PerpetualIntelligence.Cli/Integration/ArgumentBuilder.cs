/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using Microsoft.Extensions.DependencyInjection;
using PerpetualIntelligence.Cli.Commands;

namespace PerpetualIntelligence.Cli.Integration
{
    /// <summary>
    /// The default <see cref="ICommandBuilder"/>.
    /// </summary>
    public sealed class ArgumentBuilder : IArgumentBuilder
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="commandBuilder">The <see cref="ICommandBuilder"/>.</param>
        public ArgumentBuilder(ICommandBuilder commandBuilder)
        {
            this.commandBuilder = commandBuilder;
            Services = new ServiceCollection();
        }

        /// <summary>
        /// The service collection.
        /// </summary>
        public IServiceCollection Services { get; }

        /// <summary>
        /// Builds an <see cref="ArgumentDescriptor"/> and adds it to the service collection.
        /// </summary>
        /// <returns></returns>
        public ICommandBuilder Build()
        {
            // Does nothing for now.
            return commandBuilder;
        }

        private readonly ICommandBuilder commandBuilder;
    }
}
