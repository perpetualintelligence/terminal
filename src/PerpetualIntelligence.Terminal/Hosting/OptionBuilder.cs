/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.DependencyInjection;
using PerpetualIntelligence.Terminal.Commands;
using PerpetualIntelligence.Terminal.Commands.Checkers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace PerpetualIntelligence.Terminal.Hosting
{
    /// <summary>
    /// The default <see cref="ICommandBuilder"/>.
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
            ServiceProvider localSeviceProvider = Services.BuildServiceProvider();
            OptionDescriptor? optionDescriptor = localSeviceProvider.GetService<OptionDescriptor>();
            if (optionDescriptor != null)
            {
                // Custom Properties
                IEnumerable<Tuple<string, object>> customProps = localSeviceProvider.GetServices<Tuple<string, object>>();
                if (customProps.Any())
                {
                    optionDescriptor.CustomProperties = new Dictionary<string, object>();
                    customProps.All(e =>
                    {
                        optionDescriptor.CustomProperties.Add(e.Item1, e.Item2);
                        return true;
                    });
                }

                // Validation Attribute
                IEnumerable<ValidationAttribute> attributes = localSeviceProvider.GetServices<ValidationAttribute>();
                if (attributes.Any())
                {
                    optionDescriptor.ValueCheckers = attributes.Select(e => new DataValidationOptionValueChecker(e));
                }

                commandBuilder.Services.AddSingleton(optionDescriptor);
            }

            // Does nothing for now.
            return commandBuilder;
        }

        private readonly ICommandBuilder commandBuilder;
    }
}