/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using Microsoft.Extensions.DependencyInjection;
using PerpetualIntelligence.Cli.Integration;
using System;
using System.ComponentModel.DataAnnotations;

namespace PerpetualIntelligence.Cli.Extensions
{
    /// <summary>
    /// The <see cref="IArgumentBuilder"/> extension methods.
    /// </summary>
    public static class IArgumentBuilderExtensions
    {
        /// <summary>
        /// Adds a <see cref="ValidationAttribute"/> to the <see cref="IArgumentBuilder"/>
        /// </summary>
        /// <param name="builder">The <see cref="ICommandBuilder"/>.</param>
        /// <param name="args">
        /// An array of arguments that match in number, order, and type of constructor parameters for the validation
        /// attribute. If args is an empty array or null, the constructor that takes no parameters (the default
        /// constructor) is invoked. The constructor must be public.
        /// </param>
        /// <returns>The configured <see cref="ICommandBuilder"/>.</returns>-
        public static IArgumentBuilder AddValidationAttribute<TValidation>(this IArgumentBuilder builder, params object[] args) where TValidation : ValidationAttribute
        {
            ValidationAttribute validationAttribute = (ValidationAttribute)Activator.CreateInstance(typeof(TValidation), args);
            builder.Services.AddSingleton(validationAttribute);
            return builder;
        }
    }
}
