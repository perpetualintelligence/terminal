/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;

namespace OneImlx.Terminal.Commands.Declarative
{
    /// <summary>
    /// Declares an <see cref="ArgumentDescriptor"/> validation attribute.
    /// </summary>
    /// <seealso cref="ArgumentDescriptor.ValueCheckers"/>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class ArgumentValidationAttribute : Attribute
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="argumentId">The argument identifier.</param>
        /// <param name="validationAttribute">The argument validation attribute.</param>
        /// <param name="validationParams">The validation attribute parameters.</param>
        public ArgumentValidationAttribute(string argumentId, Type validationAttribute, params object[] validationParams)
        {
            ArgumentId = argumentId;
            ValidationAttribute = validationAttribute;
            ValidationParams = validationParams;
        }

        /// <summary>
        /// The argument identifier.
        /// </summary>
        public string ArgumentId { get; }

        /// <summary>
        /// The <see cref="ValidationAttribute"/> parameters.
        /// </summary>
        public object[]? ValidationParams { get; }

        /// <summary>
        /// The attribute validation attribute.
        /// </summary>
        public Type ValidationAttribute { get; }
    }
}