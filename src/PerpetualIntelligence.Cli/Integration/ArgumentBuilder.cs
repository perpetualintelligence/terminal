/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using Microsoft.Extensions.DependencyInjection;
using PerpetualIntelligence.Cli.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

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
        public ICommandBuilder Add()
        {
            ServiceProvider localSeviceProvider = Services.BuildServiceProvider();
            ArgumentDescriptor? argumentDescriptor = localSeviceProvider.GetService<ArgumentDescriptor>();
            if (argumentDescriptor != null)
            {
                // Custom Properties
                IEnumerable<Tuple<string, object>> customProps = localSeviceProvider.GetServices<Tuple<string, object>>();
                if (customProps.Any())
                {
                    argumentDescriptor.CustomProperties = new Dictionary<string, object>();
                    customProps.All(e =>
                    {
                        argumentDescriptor.CustomProperties.Add(e.Item1, e.Item2);
                        return true;
                    });
                }

                // Validation Attribute
                IEnumerable<ValidationAttribute> attributes = localSeviceProvider.GetServices<ValidationAttribute>();
                if(attributes.Any())
                {
                    argumentDescriptor.ValidationAttributes = attributes;
                }

                commandBuilder.Services.AddSingleton(argumentDescriptor);
            }

            // Does nothing for now.
            return commandBuilder;
        }

        private readonly ICommandBuilder commandBuilder;
    }
}
