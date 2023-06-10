/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;

namespace PerpetualIntelligence.Terminal.Commands.Declarative
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
        /// <param name="argId">The option identifier.</param>
        /// <param name="validationAttribute">The option validation attribute.</param>
        /// <param name="validationArgs">The validation attribute options.</param>
        public OptionValidationAttribute(string argId, Type validationAttribute, params object[] validationArgs)
        {
            ArgId = argId;
            ValidationAttribute = validationAttribute;
            ValidationArgs = validationArgs;
        }

        /// <summary>
        /// The option identifier.
        /// </summary>
        public string ArgId { get; }

        /// <summary>
        /// The validation attribute options.
        /// </summary>
        public object[]? ValidationArgs { get; }

        /// <summary>
        /// The option validation attribute.
        /// </summary>
        public Type ValidationAttribute { get; }
    }
}
