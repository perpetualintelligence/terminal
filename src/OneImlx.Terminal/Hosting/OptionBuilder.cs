/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.DependencyInjection;
using OneImlx.Terminal.Commands;
using OneImlx.Terminal.Commands.Checkers;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace OneImlx.Terminal.Hosting
{
    /// <summary>
    /// The default <see cref="IOptionBuilder"/>.
    /// </summary>
    public sealed class OptionBuilder : IOptionBuilder
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="commandBuilder">The <see cref="ICommandBuilder"/>.</param>
        public OptionBuilder(ICommandBuilder commandBuilder)
        {
            this.commandBuilder = commandBuilder;
            Services = new ServiceCollection();
        }

        /// <summary>
        /// The service collection.
        /// </summary>
        public IServiceCollection Services { get; }

        /// <summary>
        /// Builds an <see cref="OptionDescriptor"/> and adds it to the service collection.
        /// </summary>
        /// <returns></returns>
        public ICommandBuilder Add()
        {
            ServiceProvider lsp = Services.BuildServiceProvider();
            OptionDescriptor optionDescriptor = lsp.GetService<OptionDescriptor>() ?? throw new TerminalException(TerminalErrors.MissingOption, "The option builder is missing an option descriptor.");

            // Validation Attribute
            IEnumerable<ValidationAttribute> attributes = lsp.GetServices<ValidationAttribute>();
            if (attributes.Any())
            {
                optionDescriptor.ValueCheckers = attributes.Select(e => new DataValidationValueChecker<Option>(e));
            }

            commandBuilder.Services.AddSingleton(optionDescriptor);
            return commandBuilder;
        }

        private readonly ICommandBuilder commandBuilder;
    }
}