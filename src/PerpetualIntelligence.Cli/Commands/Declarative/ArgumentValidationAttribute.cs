/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using System;

namespace PerpetualIntelligence.Cli.Commands.Declarative
{
    /// <summary>
    /// Declares an <see cref="ArgumentDescriptor"/> validation attribute.
    /// </summary>
    /// <seealso cref="ArgumentDescriptor.ValidationAttributes"/>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class ArgumentValidationAttribute : Attribute
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="argId">The argument identifier.</param>
        /// <param name="validationAttribute">The argument validation attribute.</param>
        /// <param name="validationArgs">The validation attribute arguments.</param>
        public ArgumentValidationAttribute(string argId, Type validationAttribute, params object[] validationArgs)
        {
            ArgId = argId;
            ValidationAttribute = validationAttribute;
            ValidationArgs = validationArgs;
        }

        /// <summary>
        /// The argument identifier.
        /// </summary>
        public string ArgId { get; }

        /// <summary>
        /// The validation attribute arguments.
        /// </summary>
        public object[]? ValidationArgs { get; }

        /// <summary>
        /// The argument validation attribute.
        /// </summary>
        public Type ValidationAttribute { get; }
    }
}
