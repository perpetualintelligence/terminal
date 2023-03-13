/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands.Checkers
{
    /// <summary>
    /// The default <see cref="IArgumentValueChecker"/> that uses <see cref="System.ComponentModel.DataAnnotations.ValidationAttribute"/> to check an argument value.
    /// </summary>
    public sealed class DataValidationArgumentValueChecker : IArgumentValueChecker
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="validationAttribute">The validation attribute.</param>
        public DataValidationArgumentValueChecker(ValidationAttribute validationAttribute)
        {
            this.ValidationAttribute = validationAttribute ?? throw new ArgumentNullException(nameof(validationAttribute));
        }

        /// <summary>
        /// The validation attribute.
        /// </summary>
        public ValidationAttribute ValidationAttribute { get; }

        /// <inheritdoc/>
        public Task CheckAsync(Option argument)
        {
            ValidationContext context = new(argument);
            ValidationAttribute.Validate(argument.Value, context);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj is not DataValidationArgumentValueChecker objClass) { return false; }

            return objClass.ValidationAttribute.Equals(this.ValidationAttribute);
        }

        /// <inheritdoc/>
        public bool Equals(DataValidationArgumentValueChecker other)
        {
            return other.ValidationAttribute.Equals(this.ValidationAttribute);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return ValidationAttribute.GetHashCode();
        }

        /// <inheritdoc/>
        public Type GetRawType()
        {
            return ValidationAttribute.GetType();
        }

        /// <inheritdoc/>
        public static bool operator ==(DataValidationArgumentValueChecker? left, DataValidationArgumentValueChecker? right)
        {
            return EqualityComparer<DataValidationArgumentValueChecker>.Default.Equals(left, right);
        }

        /// <inheritdoc/>
        public static bool operator !=(DataValidationArgumentValueChecker? left, DataValidationArgumentValueChecker? right)
        {
            return !(left == right);
        }
    }
}