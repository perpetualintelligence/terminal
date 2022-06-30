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
    public sealed class CommandBuilder : ICommandBuilder
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="cliBuilder">The <see cref="CliBuilder"/>.</param>
        public CommandBuilder(ICliBuilder cliBuilder)
        {
            this.cliBuilder = cliBuilder;
            Services = new ServiceCollection();
        }

        /// <summary>
        /// The service collection.
        /// </summary>
        public IServiceCollection Services { get; }

        /// <summary>
        /// Builds a <see cref="CommandDescriptor"/> and adds it to the service collection.
        /// </summary>
        /// <returns></returns>
        public ICliBuilder Build()
        {
            ServiceProvider localSeviceProvider = Services.BuildServiceProvider();
            cliBuilder.Services.AddSingleton(localSeviceProvider.GetRequiredService<CommandDescriptor>());
            return cliBuilder;
        }

        private readonly ICliBuilder cliBuilder;
    }
}
