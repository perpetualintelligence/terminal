/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using Microsoft.Extensions.DependencyInjection;
using PerpetualIntelligence.Cli.Commands;
using PerpetualIntelligence.Cli.Integration;
using System.ComponentModel.DataAnnotations;

namespace PerpetualIntelligence.Cli.Extensions
{
    /// <summary>
    /// The <see cref="ICommandBuilder"/> extension methods.
    /// </summary>
    public static class ICommandBuilderExtensions
    {
        /// <summary>
        /// Adds an argument to the <see cref="ICommandBuilder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="ICommandBuilder"/>.</param>
        /// <param name="id">The argument id.</param>
        /// <param name="dataType">The argument data type.</param>
        /// <param name="description">The argument description.</param>
        /// <param name="alias">The argument alias.</param>
        /// <param name="defaultValue">The argument default value.</param>
        /// <param name="required">The argument is required.</param>
        /// <param name="disabled">The argument is disabled.</param>
        /// <param name="obsolete">The argument is obsolete.</param>
        /// <returns>The configured <see cref="ICommandBuilder"/>.</returns>
        public static IArgumentBuilder AddArgument(this ICommandBuilder builder, string id, DataType dataType, string description, string? alias = null, object? defaultValue = null, bool? required = null, bool? disabled = null, bool? obsolete = null)
        {
            ArgumentDescriptor argument = new(id, dataType, description, required, defaultValue) { Alias = alias, Disabled = disabled, Obsolete = obsolete };
            ArgumentBuilder argumentBuilder = new(builder);
            argumentBuilder.Services.AddSingleton(argument);
            return argumentBuilder;
        }

        /// <summary>
        /// Adds an argument to the <see cref="ICommandBuilder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="ICommandBuilder"/>.</param>
        /// <param name="id">The argument id.</param>
        /// <param name="customDataType">The argument custom data type.</param>
        /// <param name="description">The argument description.</param>
        /// <param name="alias">The argument alias.</param>
        /// <param name="defaultValue">The argument default value.</param>
        /// <param name="required">The argument is required.</param>
        /// <param name="disabled">The argument is disabled.</param>
        /// <param name="obsolete">The argument is obsolete.</param>
        /// <returns>The configured <see cref="ICommandBuilder"/>.</returns>
        public static IArgumentBuilder AddArgument(this ICommandBuilder builder, string id, string customDataType, string description, string? alias = null, object? defaultValue = null, bool? required = null, bool? disabled = null, bool? obsolete = null)
        {
            ArgumentDescriptor argument = new(id, customDataType, description, required, defaultValue) { Alias = alias, Disabled = disabled, Obsolete = obsolete }; ;
            ArgumentBuilder argumentBuilder = new(builder);
            argumentBuilder.Services.AddSingleton(argument);
            return argumentBuilder;
        }
    }
}
