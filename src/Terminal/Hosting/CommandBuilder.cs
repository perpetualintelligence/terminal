/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.DependencyInjection;
using PerpetualIntelligence.Terminal.Commands;
using PerpetualIntelligence.Terminal.Commands.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PerpetualIntelligence.Terminal.Hosting
{
    /// <summary>
    /// The default <see cref="ICommandBuilder"/>.
    /// </summary>
    public sealed class CommandBuilder : ICommandBuilder
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="terminalBuilder">The <see cref="ITerminalBuilder"/>.</param>
        public CommandBuilder(ITerminalBuilder terminalBuilder)
        {
            this.terminalBuilder = terminalBuilder;
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
        public ITerminalBuilder Add()
        {
            // Add the command descriptor from local to the global CLI builder.
            ServiceProvider localServiceProvider = Services.BuildServiceProvider();
            CommandDescriptor commandDescriptor = localServiceProvider.GetRequiredService<CommandDescriptor>();

            // Arguments
            IEnumerable<ArgumentDescriptor> argumentDescriptors = localServiceProvider.GetServices<ArgumentDescriptor>();
            if (argumentDescriptors.Any())
            {
                // FOMAC MUST UnicodeTextHandler is hard coded
                commandDescriptor.ArgumentDescriptors = new ArgumentDescriptors(new UnicodeTextHandler(), argumentDescriptors);
            }

            // Options
            IEnumerable<OptionDescriptor> optionDescriptors = localServiceProvider.GetServices<OptionDescriptor>();
            if (optionDescriptors.Any())
            {
                // FOMAC MUST UnicodeTextHandler is hard coded
                commandDescriptor.OptionDescriptors = new OptionDescriptors(new UnicodeTextHandler(), optionDescriptors);
            }

            // Custom Properties
            IEnumerable<Tuple<string, object>> customProps = localServiceProvider.GetServices<Tuple<string, object>>();
            if (customProps.Any())
            {
                commandDescriptor.CustomProperties = new Dictionary<string, object>();
                customProps.All(e =>
                {
                    commandDescriptor.CustomProperties.Add(e.Item1, e.Item2);
                    return true;
                });
            }

            // Owners
            OwnerIdCollection? owners = localServiceProvider.GetService<OwnerIdCollection>();
            commandDescriptor.OwnerIds = owners;

            // Tags
            TagIdCollection? tags = localServiceProvider.GetService<TagIdCollection>();
            commandDescriptor.TagIds = tags;

            // Make sure the command runner and checker are added. TODO this may add duplicate types
            terminalBuilder.Services.AddTransient(commandDescriptor.Checker ?? throw new TerminalException(TerminalErrors.InvalidConfiguration, "Checker is not configured in the command descriptor. command={0}", commandDescriptor.Id));
            terminalBuilder.Services.AddTransient(commandDescriptor.Runner ?? throw new TerminalException(TerminalErrors.InvalidConfiguration, "Runner is not configured in the command descriptor. command={0}", commandDescriptor.Id));
            terminalBuilder.Services.AddSingleton(commandDescriptor);

            return terminalBuilder;
        }

        private readonly ITerminalBuilder terminalBuilder;
    }
}