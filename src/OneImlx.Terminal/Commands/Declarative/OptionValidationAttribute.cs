/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;

namespace OneImlx.Terminal.Commands.Declarative
{
    /// <summary>
    /// Declares an <see cref="OptionDescriptor"/> validation attribute.
    /// </summary>
    /// <seealso cref="OptionDescriptor.ValueCheckers"/>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class OptionValidationAttribute : Attribute
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="optionId">The option identifier.</param>
        /// <param name="validationAttribute">The option validation attribute.</param>
        /// <param name="validationParams">The validation attribute parameters.</param>
        public OptionValidationAttribute(string optionId, Type validationAttribute, params object[] validationParams)
        {
            OptionId = optionId;
            ValidationAttribute = validationAttribute;
            ValidationParams = validationParams;
        }

        /// <summary>
        /// The option identifier.
        /// </summary>
        public string OptionId { get; }

        /// <summary>
        /// The validation attribute parameters.
        /// </summary>
        public object[]? ValidationParams { get; }

        /// <summary>
        /// The option validation attribute.
        /// </summary>
        public Type ValidationAttribute { get; }
    }
}