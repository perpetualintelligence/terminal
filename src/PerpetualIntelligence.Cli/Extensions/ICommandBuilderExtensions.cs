/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using Microsoft.Extensions.DependencyInjection;
using PerpetualIntelligence.Cli.Commands;
using PerpetualIntelligence.Cli.Hosting;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace PerpetualIntelligence.Cli.Extensions
{
    /// <summary>
    /// The <see cref="ICommandBuilder"/> extension methods.
    /// </summary>
    public static class ICommandBuilderExtensions
    {
        /// <summary>
        /// Adds a command custom property to the <see cref="ICommandBuilder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="ICommandBuilder"/>.</param>
        /// <param name="key">The custom property key.</param>
        /// <param name="value">The custom property value.</param>
        /// <returns>The configured <see cref="ICommandBuilder"/>.</returns>
        public static ICommandBuilder CustomProperty(this ICommandBuilder builder, string key, object value)
        {
            builder.Services.AddSingleton(new Tuple<string, object>(key, value));
            return builder;
        }

        /// <summary>
        /// Starts a new <see cref="IArgumentBuilder"/> definition.
        /// </summary>
        /// <param name="builder">The <see cref="ICommandBuilder"/>.</param>
        /// <param name="id">The option id.</param>
        /// <param name="dataType">The option data type.</param>
        /// <param name="description">The option description.</param>
        /// <param name="alias">The option alias.</param>
        /// <param name="defaultValue">The option default value.</param>
        /// <param name="required">The option is required.</param>
        /// <param name="disabled">The option is disabled.</param>
        /// <param name="obsolete">The option is obsolete.</param>
        /// <returns>The configured <see cref="IArgumentBuilder"/>.</returns>
        public static IArgumentBuilder DefineArgument(this ICommandBuilder builder, string id, DataType dataType, string description, string? alias = null, object? defaultValue = null, bool? required = null, bool? disabled = null, bool? obsolete = null)
        {
            OptionDescriptor option = new(id, dataType, description, required, defaultValue) { Alias = alias, Disabled = disabled, Obsolete = obsolete };
            ArgumentBuilder argumentBuilder = new(builder);
            argumentBuilder.Services.AddSingleton(option);
            return argumentBuilder;
        }

        /// <summary>
        /// Starts a new <see cref="IArgumentBuilder"/> definition.
        /// </summary>
        /// <param name="builder">The <see cref="ICommandBuilder"/>.</param>
        /// <param name="id">The option id.</param>
        /// <param name="customDataType">The option custom data type.</param>
        /// <param name="description">The option description.</param>
        /// <param name="alias">The option alias.</param>
        /// <param name="defaultValue">The option default value.</param>
        /// <param name="required">The option is required.</param>
        /// <param name="disabled">The option is disabled.</param>
        /// <param name="obsolete">The option is obsolete.</param>
        /// <returns>The configured <see cref="IArgumentBuilder"/>.</returns>
        public static IArgumentBuilder DefineArgument(this ICommandBuilder builder, string id, string customDataType, string description, string? alias = null, object? defaultValue = null, bool? required = null, bool? disabled = null, bool? obsolete = null)
        {
            OptionDescriptor option = new(id, customDataType, description, required, defaultValue) { Alias = alias, Disabled = disabled, Obsolete = obsolete }; ;
            ArgumentBuilder argumentBuilder = new(builder);
            argumentBuilder.Services.AddSingleton(option);
            return argumentBuilder;
        }

        /// <summary>
        /// Adds command tags to the <see cref="ICommandBuilder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="ICommandBuilder"/>.</param>
        /// <param name="tags">The tags.</param>
        /// <returns>The configured <see cref="ICommandBuilder"/>.</returns>
        public static ICommandBuilder Tags(this ICommandBuilder builder, params string[] tags)
        {
            if (!tags.Any())
            {
                throw new InvalidOperationException("The tags cannot be null or empty.");
            }

            builder.Services.AddSingleton(tags);
            return builder;
        }
    }
}