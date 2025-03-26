/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using OneImlx.Terminal.Commands;
using OneImlx.Terminal.Shared;

namespace OneImlx.Terminal.Hosting
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
            ServiceProvider lsp = Services.BuildServiceProvider();
            CommandDescriptor commandDescriptor = lsp.GetRequiredService<CommandDescriptor>();

            // Arguments
            IEnumerable<ArgumentDescriptor> argumentDescriptors = lsp.GetServices<ArgumentDescriptor>();
            if (argumentDescriptors.Any())
            {
                commandDescriptor.ArgumentDescriptors = new ArgumentDescriptors(terminalBuilder.TextHandler, argumentDescriptors);
            }

            // Options
            IEnumerable<OptionDescriptor> optionDescriptors = lsp.GetServices<OptionDescriptor>();
            if (optionDescriptors.Any())
            {
                commandDescriptor.OptionDescriptors = new OptionDescriptors(terminalBuilder.TextHandler, optionDescriptors);
            }

            // Custom Properties
            IEnumerable<Tuple<string, object>> customProps = lsp.GetServices<Tuple<string, object>>();
            if (customProps.Any())
            {
                commandDescriptor.CustomProperties = [];
                customProps.All(e =>
                {
                    commandDescriptor.CustomProperties.Add(e.Item1, e.Item2);
                    return true;
                });
            }

            // Owners
            // - Root command or native commands cannot have an owner
            OwnerIdCollection? owners = lsp.GetService<OwnerIdCollection>();
            if ((commandDescriptor.Type == CommandType.RootCommand || commandDescriptor.Type == CommandType.NativeCommand))
            {
                if (owners != null && owners.Count > 0)
                {
                    throw new TerminalException(TerminalErrors.InvalidCommand, "The command cannot have an owner. command_type={0} command={1}", commandDescriptor.Type, commandDescriptor.Id);
                }
            }
            commandDescriptor.OwnerIds = owners;

            // Tags
            TagIdCollection? tags = lsp.GetService<TagIdCollection>();
            commandDescriptor.TagIds = tags;

            // Add the command descriptor to the terminal builder.
            terminalBuilder.Services.AddSingleton(commandDescriptor);

            return terminalBuilder;
        }

        private readonly ITerminalBuilder terminalBuilder;
    }
}
