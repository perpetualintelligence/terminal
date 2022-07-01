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
        /// <param name="required">The argument is required.</param>
        /// <param name="description">The argument description.</param>
        /// <param name="defaultValue">The argument default value.</param>
        /// <returns>The configured <see cref="ICommandBuilder"/>.</returns>
        public static ICommandBuilder AddArgument(this ICommandBuilder builder, string id, DataType dataType, string description, bool? required = null, object? defaultValue = null)
        {
            ArgumentDescriptor argument = new(id, dataType, description, required, defaultValue);
            builder.Services.AddSingleton(argument);
            return builder;
        }

        /// <summary>
        /// Adds an argument to the <see cref="ICommandBuilder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="ICommandBuilder"/>.</param>
        /// <param name="id">The argument id.</param>
        /// <param name="customDataType">The argument custom data type.</param>
        /// <param name="description">The argument description.</param>
        /// <param name="required">The argument is required.</param>
        /// <param name="defaultValue">The argument default value.</param>
        /// <returns>The configured <see cref="ICommandBuilder"/>.</returns>
        public static ICommandBuilder AddArgument(this ICommandBuilder builder, string id, string customDataType, string description, bool? required = null, object? defaultValue = null)
        {
            ArgumentDescriptor argument = new(id, customDataType, description, required, defaultValue);
            builder.Services.AddSingleton(argument);
            return builder;
        }
    }
}
