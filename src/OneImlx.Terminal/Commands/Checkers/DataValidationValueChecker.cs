/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Commands.Checkers
{
    /// <summary>
    /// The default <see cref="IValueChecker{T}"/> that uses <see cref="System.ComponentModel.DataAnnotations.ValidationAttribute"/> to check an option value.
    /// </summary>
    public sealed class DataValidationValueChecker<TValue> : IValueChecker<TValue> where TValue : IValue
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="validationAttribute">The validation attribute.</param>
        public DataValidationValueChecker(ValidationAttribute validationAttribute)
        {
            this.ValidationAttribute = validationAttribute ?? throw new ArgumentNullException(nameof(validationAttribute));
        }

        /// <summary>
        /// The validation attribute.
        /// </summary>
        public ValidationAttribute ValidationAttribute { get; }

        /// <inheritdoc/>
        public Task CheckValueAsync(TValue value)
        {
            ValidationContext context = new(value);
            ValidationAttribute.Validate(value.Value, context);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj is not DataValidationValueChecker<TValue> objClass) { return false; }

            return objClass.ValidationAttribute.Equals(ValidationAttribute);
        }

        /// <inheritdoc/>
        public bool Equals(DataValidationValueChecker<TValue> other)
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
        public static bool operator ==(DataValidationValueChecker<TValue>? left, DataValidationValueChecker<TValue>? right)
        {
            return (left == right);
        }

        /// <inheritdoc/>
        public static bool operator !=(DataValidationValueChecker<TValue>? left, DataValidationValueChecker<TValue>? right)
        {
            return !(left == right);
        }
    }
}