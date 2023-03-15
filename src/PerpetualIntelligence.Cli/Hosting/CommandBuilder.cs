/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.DependencyInjection;
using PerpetualIntelligence.Cli.Commands;
using PerpetualIntelligence.Cli.Commands.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PerpetualIntelligence.Cli.Hosting
{
    /// <summary>
    /// The default <see cref="ICommandBuilder"/>.
    /// </summary>
    public sealed class CommandBuilder : ICommandBuilder
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="cliBuilder">The <see cref="ICliBuilder"/>.</param>
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
        public ICliBuilder Add()
        {
            // Add the command descriptor from local to the global CLI builder.
            ServiceProvider localSeviceProvider = Services.BuildServiceProvider();
            CommandDescriptor commandDescriptor = localSeviceProvider.GetRequiredService<CommandDescriptor>();

            // Options
            IEnumerable<OptionDescriptor> argumentDescriptors = localSeviceProvider.GetServices<OptionDescriptor>();
            if (argumentDescriptors.Any())
            {
                // FOMAC MUST UnicodeTextHandler is hard coded
                commandDescriptor.OptionDescriptors = new OptionDescriptors(new UnicodeTextHandler(), argumentDescriptors);
            }

            // Custom Properties
            IEnumerable<Tuple<string, object>> customProps = localSeviceProvider.GetServices<Tuple<string, object>>();
            if (customProps.Any())
            {
                commandDescriptor.CustomProperties = new Dictionary<string, object>();
                customProps.All(e =>
                {
                    commandDescriptor.CustomProperties.Add(e.Item1, e.Item2);
                    return true;
                });
            }

            // Tags
            string[]? tags = localSeviceProvider.GetService<string[]>();
            if (tags != null && tags.Any())
            {
                commandDescriptor.Tags = tags.ToArray();
            }

            // Make sure the command runner and checker are added. TODO this may add duplicate types
            cliBuilder.Services.AddTransient(commandDescriptor.Checker);
            cliBuilder.Services.AddTransient(commandDescriptor.Runner);
            cliBuilder.Services.AddSingleton(commandDescriptor);

            return cliBuilder;
        }

        private readonly ICliBuilder cliBuilder;
    }
}