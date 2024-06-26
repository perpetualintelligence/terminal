﻿/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OneImlx.Terminal.Commands;
using System;
using System.Collections.Generic;
using System.Linq;

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
            OwnerIdCollection? owners = lsp.GetService<OwnerIdCollection>();
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