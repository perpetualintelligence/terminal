/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Commands.Checkers
{
    /// <summary>
    /// The default <see cref="IOptionValueChecker"/> that uses <see cref="System.ComponentModel.DataAnnotations.ValidationAttribute"/> to check an option value.
    /// </summary>
    public sealed class DataValidationOptionValueChecker : IOptionValueChecker
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="validationAttribute">The validation attribute.</param>
        public DataValidationOptionValueChecker(ValidationAttribute validationAttribute)
        {
            this.ValidationAttribute = validationAttribute ?? throw new ArgumentNullException(nameof(validationAttribute));
        }

        /// <summary>
        /// The validation attribute.
        /// </summary>
        public ValidationAttribute ValidationAttribute { get; }

        /// <inheritdoc/>
        public Task CheckAsync(Option option)
        {
            ValidationContext context = new(option);
            ValidationAttribute.Validate(option.Value, context);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj is not DataValidationOptionValueChecker objClass) { return false; }

            return objClass.ValidationAttribute.Equals(this.ValidationAttribute);
        }

        /// <inheritdoc/>
        public bool Equals(DataValidationOptionValueChecker other)
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
        public static bool operator ==(DataValidationOptionValueChecker? left, DataValidationOptionValueChecker? right)
        {
            return (left == right);
        }

        /// <inheritdoc/>
        public static bool operator !=(DataValidationOptionValueChecker? left, DataValidationOptionValueChecker? right)
        {
            return !(left == right);
        }
    }
}