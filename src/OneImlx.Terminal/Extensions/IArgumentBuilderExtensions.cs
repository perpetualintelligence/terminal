/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.DependencyInjection;
using OneImlx.Terminal.Hosting;
using System;
using System.ComponentModel.DataAnnotations;

namespace OneImlx.Terminal.Extensions
{
    /// <summary>
    /// The <see cref="IArgumentBuilder"/> extension methods.
    /// </summary>
    public static class IArgumentBuilderExtensions
    {
        /// <summary>
        /// Adds a <see cref="ValidationAttribute{TValidation}"/> to the <see cref="IArgumentBuilder"/>
        /// </summary>
        /// <param name="builder">The <see cref="ICommandBuilder"/>.</param>
        /// <param name="args">
        /// An array of parameters that match in number, order, and type of constructor parameters for the validation
        /// attribute. If args is an empty array or null, the constructor that takes no parameters (the default
        /// constructor) is invoked. The constructor must be public.
        /// </param>
        /// <returns>The configured <see cref="ICommandBuilder"/>.</returns>
        /// -
        public static IArgumentBuilder ValidationAttribute<TValidation>(this IArgumentBuilder builder, params object[]? args) where TValidation : ValidationAttribute
        {
            return ValidationAttribute(builder, typeof(TValidation), args);
        }

        /// <summary>
        /// Adds a <see cref="ValidationAttribute{TValidation}"/> to the <see cref="IArgumentBuilder"/>
        /// </summary>
        /// <param name="builder">The <see cref="ICommandBuilder"/>.</param>
        /// <param name="validationAttribute">The validation attribute.</param>
        /// <param name="args">
        /// An array of parameters that match in number, order, and type of constructor parameters for the validation
        /// attribute. If args is an empty array or null, the constructor that takes no parameters (the default
        /// constructor) is invoked. The constructor must be public.
        /// </param>
        /// <returns>The configured <see cref="ICommandBuilder"/>.</returns>
        /// -
        public static IArgumentBuilder ValidationAttribute(this IArgumentBuilder builder, Type validationAttribute, params object[]? args)
        {
            ValidationAttribute? validationAttributeInstance = (ValidationAttribute?)Activator.CreateInstance(validationAttribute, args);
            builder.Services.AddSingleton(validationAttributeInstance!);
            return builder;
        }
    }
}